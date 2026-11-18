using System.Collections;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Modifiers;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles.Crewmate;
using Reactor.Utilities;

namespace TouMarmo.Events;

[HarmonyPatch(typeof(TimeLordRole), nameof(TimeLordRole.RpcStartRewind))]
public static class TimeLordRiskPatch
{
    private static int sequence;

    [HarmonyPostfix]
    public static void Postfix(PlayerControl timeLord)
    {
        // StopRewind can be inlined by the game runtime. Track the actual RPC that
        // starts a rewind, then resolve the risk once the host finishes it.
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost ||
            !TimeLordRewindSystem.IsRewinding || timeLord?.Data?.Role is not TimeLordRole)
            return;

        Coroutines.Start(CoResolveRewind(timeLord.PlayerId, ++sequence));
    }

    private static IEnumerator CoResolveRewind(byte sourceId, int currentSequence)
    {
        while (TimeLordRewindSystem.IsRewinding)
        {
            if (!AmongUsClient.Instance || AmongUsClient.Instance.IsGameOver ||
                LobbyBehaviour.Instance || MeetingHud.Instance || ExileController.Instance ||
                currentSequence != sequence)
                yield break;
            yield return null;
        }

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost ||
            AmongUsClient.Instance.IsGameOver || LobbyBehaviour.Instance ||
            MeetingHud.Instance || ExileController.Instance || currentSequence != sequence)
            yield break;

        var timeLord = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p.PlayerId == sourceId);
        if (timeLord?.Data?.Role is not TimeLordRole || timeLord.HasDied())
            yield break;

        var chance = Math.Clamp(OptionGroupSingleton<TimeLordRiskOptions>.Instance.DeathChance.Value, 0f, 100f);
        if (chance <= 0f || System.Random.Shared.Next(100) >= chance)
            yield break;

        var candidates = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != timeLord && !p.HasDied() && !p.Data.Disconnected &&
                SoulguardRole.FindLivingGuard(p) == null && !p.HasModifier<InvulnerabilityModifier>() &&
                !(p.Data.Role is SoulguardRole { ProtectedPlayer: { } linked } && !linked.HasDied()) &&
                !(OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie &&
                  p.TryGetModifier<LoverModifier>(out var lover) && lover.OtherLover != null &&
                  !lover.OtherLover.HasDied()))
            .ToArray();
        if (candidates.Length == 0)
            yield break;

        var victim = candidates[System.Random.Shared.Next(candidates.Length)];
        timeLord.RpcSpecialMurder(victim, MeetingCheck.OutsideMeeting,
            isIndirect: true, ignoreShield: true, resetKillTimer: false,
            teleportMurderer: false, showKillAnim: false,
            causeOfDeath: "TimeLordRewind");
    }
}
