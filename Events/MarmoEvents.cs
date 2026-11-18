using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Voting;
using TouMarmo.Modifiers;
using TouMarmo.Assets;
using TouMarmo.Roles;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Networking;
using UnityEngine;

namespace TouMarmo.Events;

public static class MarmoEvents
{
    [RegisterEvent(1100)]
    public static void KemadaVote(BeforeVoteEvent @event)
    {
        if (@event.IsCancelled || !@event.Player.HasModifier<KemadaModifier>() || @event.Player.HasDied())
        {
            return;
        }

        var candidates = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.HasDied() && !p.Data.Disconnected && p != @event.Player)
            .ToArray();
        var selected = candidates.Length > 0
            ? candidates[System.Random.Shared.Next(candidates.Length)].PlayerId
            : (byte)253;
        VotingUtils.RpcCastVote(@event.Player, @event.Player.PlayerId, selected);
        @event.Cancel();
    }

    [RegisterEvent(1100)]
    public static void ProtectFromMurder(BeforeMurderEvent @event)
    {
        // ProtectionRuntime resolves Dirtek before consumable shields.
    }

    [RegisterEvent(1100)]
    public static void ProtectFromExile(ProcessVotesEvent @event)
    {
        if (@event.ExiledPlayer?.Object is { } target && !ProtectionRuntime.Bypass(target) && SoulguardRole.FindLivingGuard(target) != null)
        {
            @event.ExiledPlayer = null;
        }
    }

    [RegisterEvent]
    public static void NextRound(RoundStartEvent @event)
    {
        SoulguardRole.AssignPermanentTargets();
        foreach (var gambler in CustomRoleUtils.GetActiveRolesOfType<GamblerRole>())
        {
            if (!@event.TriggeredByIntro)
            {
                gambler.StartNextRound();
            }
        }
    }

    [RegisterEvent(400)]
    public static void LinkedDeathAndWager(PlayerDeathEvent @event)
    {
        foreach (var gambler in CustomRoleUtils.GetActiveRolesOfType<GamblerRole>())
        {
            if (!MeetingHud.Instance && !ExileController.Instance && !gambler.Player.HasDied() &&
                gambler.ActiveMarks.Contains(@event.Player.PlayerId))
            {
                gambler.ReadyToShoot = true;
                gambler.ActiveMarks.Clear();
                if (gambler.Player.AmOwner)
                {
                    Helpers.CreateAndShowNotification(
                        MiraLocaleManager.Get("TouMarmo.Role.Gambler.ReadyNotice", "Your Cash Out shot is ready."),
                        Color.white, new Vector3(0f, 1f, -20f), spr: MarmoAssets.CashOut.LoadAsset());
                }
            }
        }
    }

    [RegisterEvent]
    public static void SuccessfulGamblerShot(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is GamblerRole gambler &&
            gambler.LastShotTargetId == @event.Target.PlayerId && @event.Target.HasDied())
        {
            gambler.HasWon = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class SoulguardExilePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance) => ProtectionRuntime.Bypass(__instance) || SoulguardRole.FindLivingGuard(__instance) == null;
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class DirtekDirectMurderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        if (ProtectionRuntime.Bypass(target) || SoulguardRole.FindLivingGuard(target) is not { } guard) return true;
        if (resultFlags.HasFlag(MurderResultFlags.Succeeded))
            ProtectionRuntime.ShowLocal(__instance, target, ProtectionKind.Dirtek, guard.Player);
        return false;
    }
}

[HarmonyPatch(typeof(AssassinModifier), nameof(AssassinModifier.IsModifierGuessable))]
public static class KemadaGuessPatch
{
    [HarmonyPostfix]
    public static void Postfix(BaseModifier baseModifier, ref bool __result)
    {
        if (baseModifier is KemadaModifier)
        {
            __result = MiraAPI.GameOptions.OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.KemadaGuessable.Value;
        }
    }
}
