using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs;
using TownOfUs.Networking;
using UnityEngine;

namespace TouMarmo.Modules.Sweets;

public static class SweetsRuntime
{
    private static readonly BepInEx.Logging.ManualLogSource Log =
        BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.Sweets");

    public sealed class Cycle
    {
        public readonly HashSet<byte> Flowers=new();
        public readonly HashSet<byte> Notified=new();
        public int FlowersGiven;
        public float ReadyAt;
    }
    private static readonly Dictionary<byte,Cycle> cycles=new();
    public static Cycle For(PlayerControl player)
    {
        var id=player ? player.PlayerId : byte.MaxValue;
        if(!cycles.TryGetValue(id,out var cycle)) cycles[id]=cycle=new();
        return cycle;
    }
    public static HashSet<byte> Flowers => For(PlayerControl.LocalPlayer).Flowers;
    public static int FlowersGiven => For(PlayerControl.LocalPlayer).FlowersGiven;
    public static float CooldownRemaining => Mathf.Max(0,For(PlayerControl.LocalPlayer).ReadyAt-Time.time);

    public static SweetsOptions Options => OptionGroupSingleton<SweetsOptions>.Instance;

    public static float Distance => 2.25f;

    public static int MaxFlowersCap => Mathf.Max(1, (int)Options.MaxFlowers.Value);

    // Max flowers setting is only a cap; the actual requirement scales down to how many
    // non-Impostor players (other than Sweets) can actually receive a flower.
    public static int MaxFlowers => RequiredFlowers;

    public static int RequiredFlowers
    {
        get
        {
            var possible = 0;
            if (GameData.Instance?.AllPlayers != null)
            {
                foreach (var p in GameData.Instance.AllPlayers)
                {
                    if (p == null || p.Disconnected) continue;
                    var obj = p.Object;
                    if (!obj || !MarmoSession.Alive(obj)) continue;
                    if (obj.Data.Role is SweetsRole) continue;
                    // Impostors never count as flower recipients.
                    if (obj.IsImpostorAligned()) continue;
                    possible++;
                }
            }

            if (possible <= 0) possible = 1;
            return Mathf.Clamp(MaxFlowersCap, 1, possible);
        }
    }

    public static bool IsSweets(PlayerControl? p) => p && p!.Data?.Role is SweetsRole;

    // The ability button morphs into Pollinate once every required flower has been handed out.
    public static bool PollinateMode => FlowersGiven > 0 && FlowersGiven >= RequiredFlowers;

    public static bool CanGive(PlayerControl sweets, PlayerControl target)
    {
        var cycle=For(sweets);
        if (!MarmoSession.Alive(sweets) || !MarmoSession.Alive(target)) return false;
        if (!IsSweets(sweets) || !MarmoSession.InRound || Time.time < cycle.ReadyAt) return false;
        if (sweets == target) return false;
        // A flower can never be given to another Impostor-aligned player.
        if (target.IsImpostorAligned()) return false;
        if (cycle.Flowers.Contains(target.PlayerId)) return false;
        if (cycle.FlowersGiven >= RequiredFlowers) return false;
        return Vector2.Distance(sweets.GetTruePosition(), target.GetTruePosition()) <= Distance;
    }

    public static bool CanPollinate(PlayerControl sweets)
    {
        var cycle=For(sweets);
        if (!IsSweets(sweets) || !MarmoSession.Alive(sweets) || !MarmoSession.InRound || Time.time < cycle.ReadyAt) return false;
        if (!(cycle.FlowersGiven > 0 && cycle.FlowersGiven >= RequiredFlowers)) return false;
        // Pollinate is only worth using while at least one flowered player is still alive.
        return true; // Empty living recipients still allow clearing the completed cycle.
    }

    [MethodRpc((uint)SweetsRpc.FlowerRequest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFlowerRequest(PlayerControl sweets, PlayerControl target)
    {
        if (!MarmoSession.IsHost || !CanGive(sweets, target)) return;
        RpcGiveFlower(sweets, target);
    }

    [MethodRpc((uint)SweetsRpc.GiveFlower, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGiveFlower(PlayerControl sweets, PlayerControl target)
    {
        var cycle=For(sweets);
        if (!IsSweets(sweets) || !MarmoSession.Alive(target)) return;
        if (target.IsImpostorAligned()) return;
        if (!cycle.Flowers.Add(target.PlayerId)) return;
        cycle.FlowersGiven++;
        cycle.ReadyAt = Time.time + Options.FlowerCooldown.Value;
        BigBrainRuntime.Record(sweets, target, false);
        Log.LogInfo($"Sweets {sweets.PlayerId} gave a flower to {target.PlayerId} ({cycle.FlowersGiven}/{RequiredFlowers}).");
    }

    [MethodRpc((uint)SweetsRpc.PollinateRequest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPollinateRequest(PlayerControl sweets)
    {
        var cycle=For(sweets);
        if (!MarmoSession.IsHost || !CanPollinate(sweets)) return;

        var victims = new List<PlayerControl>();
        foreach (var id in cycle.Flowers)
        {
            var p = MarmoSession.Player(id);
            if (!MarmoSession.Alive(p)) continue;
            if (AuditRules.RollSucceeds(UnityEngine.Random.Range(0f, 100f), Options.PollinatedKillChance.Value)) victims.Add(p!);
        }

        var flowered = cycle.Flowers.Count;
        RpcPollinate(sweets);
        Log.LogInfo($"Sweets {sweets.PlayerId} pollinated {flowered} flower(s); {victims.Count} died.");

        if (victims.Count > 0)
        {
            sweets.RpcSpecialMultiMurder(victims, MeetingCheck.OutsideMeeting, isIndirect: true,
                resetKillTimer: false, teleportMurderer: false, showKillAnim: false,
                playKillSound: false, causeOfDeath: "Sweets");
        }
    }

    [MethodRpc((uint)SweetsRpc.Pollinate, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPollinate(PlayerControl sweets)
    {
        var cycle=For(sweets);
        if (!IsSweets(sweets)) return;
        // Reset state so Sweets can give flowers and pollinate again (loop).
        cycle.Flowers.Clear();
        cycle.Notified.Clear();
        cycle.FlowersGiven = 0;
        
        cycle.ReadyAt = Time.time + Options.FlowerCooldown.Value;
    }

    [RegisterEvent]
    public static void Meeting(StartMeetingEvent e)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || !HudManager.Instance || !HudManager.Instance.Chat) return;

        // The recipient learns about the flower now - but never who Sweets is.
        if (cycles.Values.Any(c => c.Flowers.Contains(local.PlayerId) && c.Notified.Add(local.PlayerId)))
        {
            var msg = MiraLocaleManager.Get("TouMarmo.Role.Sweets.FlowerNotice", "You received a flower.");
            MiscUtils.AddFakeChat(local.Data, $"<color=#{ColorUtility.ToHtmlStringRGB(TownOfUsColors.Impostor)}>Sweets</color>", msg, false, true);
        }

        // Sweets privately sees who is currently holding a flower.
        if (IsSweets(local) && local.AmOwner)
        {
            var names = Flowers.Select(MarmoSession.Player).Where(p => p).Select(p => p!.Data.PlayerName).ToArray();
            if (names.Length > 0)
            {
                var msg = MiraLocaleManager.Get("TouMarmo.Role.Sweets.FlowerList", "Flowers given:") + " " + string.Join(", ", names);
                MiscUtils.AddFakeChat(local.Data, $"<color=#{ColorUtility.ToHtmlStringRGB(TownOfUsColors.Impostor)}>Sweets</color>", msg, false, true);
            }
        }
    }

    public static void Clear() => cycles.Clear();
}
