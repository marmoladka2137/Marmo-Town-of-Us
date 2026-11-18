// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class MercenaryEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (!@event.Player.AmOwner || @event.Player.Data.Role is not MercenaryRole ||
            !OptionGroupSingleton<MercenaryOptions>.Instance.TaskUses)
        {
            return;
        }

        var button = CustomButtonSingleton<MercenaryGuardButton>.Instance;
        ++button.UsesLeft;
        button.SetUses(button.UsesLeft);
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        button?.ResetButtonCooldown(true);

        if (source.Data.Role is WerewolfRole)
        {
            CustomButtonSingleton<WerewolfRampageButton>.Instance.ResetCooldownAndOrEffect();
        }

        source.SetKillTimer(source.GetReducedKillCooldown());
    }
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        // only check if this interaction was via a custom button
        CheckForMercenaryGuard(@event, source, target);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {

        var source = @event.Source;
        var target = @event.Target;

        // only check if this interaction was via the standard kill button
        if (TutorialManager.InstanceExists || source.Data.Role is ICustomRole { Configuration.UseVanillaKillButton: true } ||
            (source.Data.Role is not ICustomRole && source.IsImpostor()))
        {
            CheckForMercenaryGuard(@event, source, target);
        }
    }

    private static void CheckForMercenaryGuard(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (!ProtectionRuntime.MayDefend(@event, target)) return;
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!target.TryGetModifier<MercenaryGuardModifier>(out var guardMod))
        {
            return;
        }

        var mercOpts = OptionGroupSingleton<MercenaryOptions>.Instance;

        var noAttack = (!TutorialManager.InstanceExists && target.PlayerId == source.PlayerId ||
                        @event is BeforeMurderEvent { IgnoreDefense: true } ||
                        @event is ExtendedMiraButtonClickEvent { IgnoreDefense: true } ||
                        source.HasModifier<InvulnerabilityModifier>() ||
                        source.HasModifier<VeteranAlertModifier>() ||
                        @event is not BeforeMurderEvent);

        var isAttack = false;
        CustomActionButton<PlayerControl>? button = null;
        if (@event is MiraButtonClickEvent clickEvent)
        {
            var button2 = clickEvent.Button as CustomActionButton<PlayerControl>;
            var target2 = button2?.Target;

            if (target2 != null && button2 != null && button2.CanClick())
            {
                button = button2;
                isAttack = button2 is TownOfUs.Buttons.IKillButton;
            }
        }
        if (mercOpts.GuardProtection.Value && (!noAttack || isAttack))
        {
            if (!ProtectionRuntime.Block(@event, source, target, ProtectionKind.Mercenary, target.GetModifier<MercenaryGuardModifier>()?.Mercenary)) return ;
            if (source.AmOwner)
            {
                ResetButtonTimer(source, button);
            }
        }

        var mercenary = guardMod.Mercenary;

        if (mercenary != null && (TutorialManager.InstanceExists || source.AmOwner))
        {
            MercenaryRole.RpcGuarded(mercenary, target, mercOpts.GuardProtection.Value && (!noAttack || isAttack));
        }
    }
}