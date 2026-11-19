using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TouMarmo.Buttons;
using TouMarmo.Modifiers;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Modules;
using TownOfUs.Roles;
using UnityEngine;

namespace TouMarmo.Modules.Exorcist;

public sealed class ExorcistReveal(byte killerId, string killerName, string killerRole)
{
    public byte KillerId { get; } = killerId;
    public string KillerName { get; } = killerName;
    public string KillerRole { get; } = killerRole;
}

public static class ExorcistRuntime
{
    public static ExorcistOptions Options => OptionGroupSingleton<ExorcistOptions>.Instance;

    public sealed class State
    {
        public int Used;
        public float ReadyAt;
        public float DragReadyAt;
        public ExorcistReveal? Pending;
    }
    private static readonly Dictionary<byte, State> States = new();
    private static readonly Dictionary<byte, byte> Carriers = new();
    private static State For(PlayerControl p)
    {
        if (!States.TryGetValue(p.PlayerId, out var s)) States[p.PlayerId] = s = new();
        return s;
    }
    public static int PrayersUsed => PlayerControl.LocalPlayer ? For(PlayerControl.LocalPlayer).Used : 0;
    public static float PrayerCooldownRemaining(PlayerControl player) => Mathf.Max(0, For(player).ReadyAt - Time.time);

    public static bool IsExorcist(PlayerControl? p) => p && p!.Data?.Role is ExorcistRole;

    public static bool CanUsePrayer(PlayerControl exorcist)
    {
        if (!MarmoSession.Alive(exorcist) || !IsExorcist(exorcist) || !MarmoSession.InRound) return false;
        if (For(exorcist).Used >= (int)Options.PrayerUses.Value || Time.time < For(exorcist).ReadyAt) return false;
        return true;
    }

    public static int TotalUnreportedBodies
    {
        get
        {
            var bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            if (bodies == null) return 0;
            var count = 0;
            foreach (var body in bodies)
            {
                if (!body || body.Reported) continue;
                count++;
            }
            return count;
        }
    }

    // Required bodies is based on ALL available unreported bodies on the map, capped at 3.
    // NOT based on nearby bodies - if there are 3 bodies on the map, you need 3.
    public static int RequiredBodies => Mathf.Clamp(Mathf.Min(3, TotalUnreportedBodies), 1, 3);

    public static List<DeadBody> BodiesNear(PlayerControl exorcist)
    {
        var bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().Where(b => b && !b.Reported).ToArray();
        var positions = bodies.Select(b => new System.Numerics.Vector2(b.TruePosition.x, b.TruePosition.y)).ToArray();
        var center = exorcist.GetTruePosition();
        return BodyGroupRules.Select(positions, new System.Numerics.Vector2(center.x, center.y),
            RequiredBodies, Options.BodyDetectionRadius.Value, 1.5f).Select(i => bodies[i]).ToList();
    }

    private static bool TryBuildReveal(PlayerControl exorcist, List<DeadBody> bodies, out ExorcistReveal? reveal)
    {
        reveal = null;
        // Collect qualifying killers among the nearby bodies (Impostor or Neutral Killing).
        var qualifying = new List<(byte VictimId, byte KillerId)>();
        foreach (var body in bodies)
        {
            var victimId = body.ParentId;
            if (ProtectionRuntime.BondDeaths.Contains(victimId)) continue; // A forced bond death is not an attack by its fallback RPC sender.
            var record = GameHistory.KilledPlayers.LastOrDefault(x => x.VictimId == victimId);
            if (record == null) continue;
            var killer = GameData.Instance?.GetPlayerById(record.KillerId)?.Object;
            if (!killer || killer.Data?.Role == null) continue;
            var historicalRole = killer.GetRoleWhenAlive();
            if (historicalRole == null) continue;
            var isQualifying = historicalRole.IsImpostor;
            if (!isQualifying && historicalRole is ITownOfUsRole tou)
                isQualifying = tou.RoleAlignment == RoleAlignment.NeutralKilling;
            if (isQualifying) qualifying.Add((victimId, record.KillerId));
        }

        if (qualifying.Count == 0) return false;

        var pick = qualifying[UnityEngine.Random.Range(0, qualifying.Count)];
        var killerPc = GameData.Instance?.GetPlayerById(pick.KillerId)?.Object;
        if (!killerPc || killerPc.Data?.Role == null) return false;

        var killerName = killerPc.Data!.PlayerName ?? "Unknown";
        var role = killerPc.GetRoleWhenAlive();
        var killerRole = role?.NiceName ?? "Unknown";
        try { if (role != null) killerRole = role.GetRoleName(); } catch { /* fallback to NiceName */ }

        reveal = new ExorcistReveal(pick.KillerId, killerName, killerRole);
        return true;
    }

    [MethodRpc((uint)ExorcistRpc.PrayerRequest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPrayerRequest(PlayerControl exorcist)
    {
        if (!MarmoSession.IsHost || !CanUsePrayer(exorcist)) return;
        var bodies = BodiesNear(exorcist);
        if (bodies.Count < RequiredBodies)
        {
            RpcFeedback(exorcist, false, "Bring the required bodies close together.");
            return;
        }
        if (!TryBuildReveal(exorcist, bodies, out var reveal) || reveal == null)
        {
            RpcFeedback(exorcist, false, "No qualifying killer was found in these bodies.");
            return;
        }
        RpcPrayer(exorcist, reveal.KillerId, reveal.KillerName, reveal.KillerRole);
    }

    [MethodRpc((uint)ExorcistRpc.Prayer, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPrayer(PlayerControl exorcist, byte killerId, string killerName, string killerRole)
    {
        if (!IsExorcist(exorcist)) return;
        var state = For(exorcist);
        state.Used++;
        state.ReadyAt = Time.time + Options.PrayerCooldown.Value;
        state.Pending = new ExorcistReveal(killerId, killerName, killerRole);
        // Runs on every peer, but feedback is private to the actual Exorcist.
        ShowFeedback(exorcist, true, "Prayer successful. The truth will be revealed at the next meeting.");
    }

    [MethodRpc(1136, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFeedback(PlayerControl exorcist, bool success, string message) => ShowFeedback(exorcist, success, message);

    private static void ShowFeedback(PlayerControl exorcist, bool success, string message)
    {
        if (!exorcist.AmOwner) return;
        var notification = Helpers.CreateAndShowNotification(message, success ? Color.green : Color.red, new Vector3(0, 1, -20));
        notification.AdjustNotification();
        Coroutines.Start(MiscUtils.CoFlash(success ? Color.green : Color.red, .35f, .22f));
        if (success) TouAudio.PlaySound(TouAudio.MayorRevealSound, 1f);
    }

    [RegisterEvent]
    public static void Meeting(StartMeetingEvent e)
    {
        foreach (var pair in States)
        {
            var reveal = pair.Value.Pending;
            pair.Value.Pending = null;
            var exorcist = MarmoSession.Player(pair.Key);
            if (reveal == null || !MarmoSession.Alive(exorcist) || !exorcist!.AmOwner || !IsExorcist(exorcist)) continue;
            Coroutines.Start(ShowMeetingReveal(exorcist, reveal));
        }
    }

    private static System.Collections.IEnumerator ShowMeetingReveal(PlayerControl exorcist, ExorcistReveal reveal)
    {
        yield return null;
        if (!MeetingHud.Instance || !MarmoSession.Alive(exorcist) || !exorcist.AmOwner || (!HudManager.Instance || !HudManager.Instance.Chat)) yield break;
        var msg = $"Prayer revealed: {reveal.KillerName} was {reveal.KillerRole}.";
        MiscUtils.AddFakeChat(exorcist.Data, "<color=#B19CD9>Exorcist</color>", msg, true, true);
    }

    [MethodRpc((uint)ExorcistRpc.DragRequest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDragging(PlayerControl playerControl, byte bodyId)
    {
        if (!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(playerControl) || !IsExorcist(playerControl)) return;
        if (Time.time < For(playerControl).DragReadyAt) return;
        var body = Helpers.GetBodyById(bodyId);
        if (Carriers.TryGetValue(bodyId, out var ownerId) && !MarmoSession.Alive(MarmoSession.Player(ownerId))) Carriers.Remove(bodyId);
        if (!body || body.Reported || Carriers.ContainsKey(bodyId) || playerControl.HasModifier<ExorcistDragModifier>() ||
            Vector2.Distance(playerControl.GetTruePosition(), body.TruePosition) > playerControl.MaxReportDistance / 4f + .15f ||
            PhysicsHelpers.AnythingBetween(playerControl.Collider, playerControl.Collider.bounds.center, body.TruePosition, Constants.ShipAndAllObjectsMask, false)) return;
        RpcStartDrag(playerControl, bodyId);
    }

    [MethodRpc((uint)ExorcistRpc.StartDrag, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDrag(PlayerControl playerControl, byte bodyId)
    {
        if (!MarmoSession.Alive(playerControl) || !IsExorcist(playerControl)) return;
        Carriers[bodyId] = playerControl.PlayerId;
        playerControl.AddModifier(new ExorcistDragModifier(bodyId));
        if (playerControl.AmOwner) CustomButtonSingleton<ExorcistDragDropButton>.Instance.SetDrop();
    }

    [MethodRpc((uint)ExorcistRpc.DropRequest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStopDragging(PlayerControl playerControl, Vector2 dropLocation)
    {
        if (!MarmoSession.IsHost || !playerControl.TryGetModifier<ExorcistDragModifier>(out var drag) || !drag.DeadBody) return;
        // Host picks the actual body position, rather than trusting an arbitrary client coordinate.
        RpcStopDrag(playerControl, drag.BodyId, drag.DeadBody!.TruePosition);
    }

    [MethodRpc((uint)ExorcistRpc.StopDrag, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStopDrag(PlayerControl playerControl, byte bodyId, Vector2 position)
    {
        Carriers.Remove(bodyId);
        For(playerControl).DragReadyAt = Time.time + CustomButtonSingleton<ExorcistDragDropButton>.Instance.Cooldown;
        var body = Helpers.GetBodyById(bodyId);
        if (body) body.transform.position = new Vector3(position.x, position.y, position.y / 1000f);
        if (playerControl.TryGetModifier<ExorcistDragModifier>(out var drag)) playerControl.RemoveModifier(drag);
        if (playerControl.AmOwner)
        {
            var button = CustomButtonSingleton<ExorcistDragDropButton>.Instance;
            button.SetDrag(); button.Timer = button.Cooldown;
        }
    }

    public static void ReleaseBody(byte id) => Carriers.Remove(id);

    public static void Clear()
    {
        States.Clear(); Carriers.Clear();
    }

    public static void DropIfDragging(PlayerControl p)
    {
        if (!p) return;
        foreach (var pair in Carriers.Where(x => x.Value == p.PlayerId).ToArray()) Carriers.Remove(pair.Key);
        if (p.TryGetModifier<ExorcistDragModifier>(out var drag))
        {
            p.RemoveModifier(drag);
        }
    }
}
