using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class ExecutionerEvents
{
    [RegisterEvent(0)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason is DeathReason.Exile)
        {
            var victim = @event.Player;
            if (!victim.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
            {
                return;
            }

            var exe = GameData.Instance.GetPlayerById(exeMod.OwnerId).Object;
            if (exe != null && !exe.HasDied() && exe.Data.Role is ExecutionerRole exeRole && exeRole.AboutToWin)
            {
                if (victim.IsCrewmate())
                {
                    exeRole.TargetVoted = true;
                }
                else
                {
                    exeRole.TargetVotedAsEvil = true;
                }
            }
        }
        else
        {
            CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetDeath(@event.Player));
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var winOption = OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin;
        
        var exe = CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>()
            .FirstOrDefault(x => x.AboutToWin && !x.Player.HasDied());

        var evilTarget = exe != null && exe.Target != null && !exe.Target.IsCrewmate();
        if (evilTarget)
        {
            winOption = ExeWinOptions.Nothing;
        }
        else if (winOption is ExeWinOptions.EndsGame)
        {
            return;
        }

        if (exe != null)
        {
            var victim = exe.Target!;
            if (victim.IsCrewmate())
            {
                exe.TargetVoted = true;
            }
            else
            {
                exe.TargetVotedAsEvil = true;
            }
            if (exe.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.ExecutionerWonSelf").Replace("<role>", $"{TownOfUsColors.Executioner.ToTextColor()}{exe.GetRoleName()}</color>")}</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.AdjustNotification();

                PlayerControl.LocalPlayer.DelayExile();

                GameHistory.RpcUpdateLocalDeathHandler(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer,
                    "DiedToWinning", HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }
            else
            {
                string message;
                LoadableAsset<Sprite> icon;

                if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeAnonymizeWin.Value)
                {
                    message = MiraLocaleManager.Get("TouNeutAnonymousVictoryMessage");
                    icon = TouRoleIcons.Neutral;
                }
                else
                {
                    message = $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.ExecutionerWonOther")
                        .Replace("<role>", $"{TownOfUsColors.Executioner.ToTextColor()}{exe.GetRoleName()}</color>")}</b>";
                    icon = TouRoleIcons.Executioner;
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    message.Replace("<player>", exe.Player.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

                notif1.AdjustNotification();
            }
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled == null || !exiled.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
        {
            return;
        }

        var exe = GameData.Instance.GetPlayerById(exeMod.OwnerId).Object;
        if (exe != null && !exe.HasDied() && exe.Data.Role is ExecutionerRole exeRole)
        {
            exeRole.AboutToWin = true;
            if (!PlayerControl.LocalPlayer.IsHost())
            {
                if (exiled.IsCrewmate())
                {
                    exeRole.TargetVoted = true;
                }
                else
                {
                    exeRole.TargetVotedAsEvil = true;
                }
            }
        }
    }
}
