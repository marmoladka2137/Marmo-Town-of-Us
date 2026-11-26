using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
namespace TouMarmo.Modules.Gravedigger;

public static class GravediggerRoleSetup
{
    // Button instances exist before inheritance. Their previous uses, paused timers and
    // meeting visibility are not initialized by PlayerControl.ChangeRole.
    public static HashSet<object>? CaptureLocal(PlayerControl owner)
    {
        if (!owner.AmOwner) return null;
        var role = owner.Data.Role;
        return CustomButtonManager.Buttons.Where(b => b.Enabled(role)).Cast<object>()
            .Concat(MeetingButtonManager.UntargetedButtons.Where(b => b.Enabled(role)))
            .Concat(MeetingButtonManager.TargetedButtons.Where(b => b.Enabled(role))).ToHashSet();
    }

    public static void ActivateLocal(PlayerControl owner, HashSet<object>? previous)
    {
        if (!owner || !owner.AmOwner || !owner.Data || !owner.Data.Role) return;
        var role = owner.Data.Role;
        foreach (var button in CustomButtonManager.Buttons.Where(b => b.Enabled(role) && previous?.Contains(b) != true))
        {
            button.ResetCooldownAndOrEffect();
            button.TimerPaused = false;
            button.SetUses(button.MaxUses);
            button.Timer = button.InitialCooldown;
        }
        if (MeetingHud.Instance)
        {
            foreach (var button in MeetingButtonManager.UntargetedButtons)
            {
                var enabled = button.Enabled(role);
                if (button.Button) button.Button!.gameObject.SetActive(enabled);
                if (!enabled || previous?.Contains(button) == true) continue;
                button.SetUses(button.MaxUses);
                button.UsesSoFar = 0;
                button.TimerPaused = false;
                button.Timer = button.InitialCooldown;
                button.UpdateHandler(MeetingHud.Instance!);
            }
            foreach (var button in MeetingButtonManager.TargetedButtons.Where(b => b.Enabled(role) && previous?.Contains(b) != true))
            {
                button.UsesLeft = button.MaxUses;
                button.Timer = button.InitialCooldown;
            }
        }
        if (HudManager.Instance)
            HudManager.Instance!.SetHudActive(owner, role, !MeetingHud.Instance && !ExileController.Instance);
    }

    public static bool CanAssign(PlayerControl owner, RoleBehaviour role) => role is not ExecutionerRole ||
        PlayerControl.AllPlayerControls.ToArray().Any(p => p != owner && MarmoSession.Alive(p) && p.IsCrewmate() &&
            !p.HasModifier<AllianceGameModifier>() && !p.HasModifier<ExecutionerTargetModifier>() &&
            !p.HasModifier<GuardianAngelTargetModifier>() && p.Data.Role is not SpectatorRole);
    // Role changes alone do not assign win-condition targets. Assign only this new role,
    // not AssignTargets(), which reassigns targets of unrelated existing players.
    public static IEnumerator Assign(PlayerControl owner, ushort roleId)
    {
        yield return null; // Broadcast inheritance before sending target/modifier RPCs.
        if (!MarmoSession.IsHost || !MarmoSession.Alive(owner) || (ushort)owner.Data.Role.Role != roleId) yield break;
        var players = PlayerControl.AllPlayerControls.ToArray().Where(p => p != owner && MarmoSession.Alive(p) && p.Data.Role is not SpectatorRole).ToArray();
        if (owner.Data.Role is ExecutionerRole)
        {
            var crew = players.Where(p => p.IsCrewmate() && !p.HasModifier<AllianceGameModifier>() &&
                !p.HasModifier<ExecutionerTargetModifier>() && !p.HasModifier<GuardianAngelTargetModifier>()).ToArray();
            if (crew.Length > 0) ExecutionerRole.RpcSetExeTarget(owner, crew[System.Random.Shared.Next(crew.Length)]);
        }
        else if (owner.Data.Role is FairyRole)
        {
            var evil = System.Random.Shared.Next(100) < (int)OptionGroupSingleton<FairyOptions>.Instance.EvilTargetPercent;
            var targets = players.Where(p => !p.HasModifier<AllianceGameModifier>() && !p.HasModifier<ExecutionerTargetModifier>() &&
                (evil ? p.IsImpostor() || p.Is(TownOfUs.Roles.RoleAlignment.NeutralKilling) : p.IsCrewmate())).ToArray();
            if (targets.Length == 0) targets = players;
            if (targets.Length > 0) FairyRole.RpcSetFairyTarget(owner, targets[System.Random.Shared.Next(targets.Length)]);
        }
        else if (owner.Data.Role is InquisitorRole)
        {
            var targets = players.OrderBy(_ => System.Random.Shared.Next())
                .Take((int)OptionGroupSingleton<InquisitorOptions>.Instance.AmountOfHeretics).ToArray();
            foreach (var target in targets) InquisitorRole.RpcAddInquisTarget(owner, target);
        }
        else if (owner.Data.Role is PlaguebearerRole || owner.Data.Role is PestilenceRole)
        {
            if (!owner.HasModifier<PlaguebearerInfectedModifier>()) owner.RpcAddModifier<PlaguebearerInfectedModifier>(owner.PlayerId);
        }
    }
}
