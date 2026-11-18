using TouMarmo.Modifiers;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;

namespace TouMarmo.Events;

public static class BalloonEvents
{
    [RegisterEvent]
    public static void OnAbility(TouAbilityEvent @event)
    {
        if (@event.Target is PlayerControl target)
        {
            GiveBalloon(@event.Player, target);
        }

        if (@event.Target2 is PlayerControl secondTarget)
        {
            GiveBalloon(@event.Player, secondTarget);
        }
    }

    [RegisterEvent]
    public static void OnCustomButton(MiraButtonClickEvent @event)
    {
        if (@event.Button is CustomActionButton<PlayerControl> { Target: { } target } button &&
            button.CanClick())
        {
            GiveBalloon(PlayerControl.LocalPlayer, target);
        }
    }

    [RegisterEvent]
    public static void OnVanillaButton(VanillaButtonClickEvent @event)
    {
        if (@event.PlayerTarget != null)
        {
            GiveBalloon(PlayerControl.LocalPlayer, @event.PlayerTarget);
        }
    }

    [RegisterEvent]
    public static void OnMurderAttempt(BeforeMurderEvent @event)
    {
        GiveBalloon(@event.Source, @event.Target);
    }

    private static void GiveBalloon(PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null || !source.AmOwner || source == target ||
            !target.HasModifier<BalloonModifier>() || source.HasModifier<BalloonHeadModifier>())
        {
            return;
        }

        source.RpcAddModifier<BalloonHeadModifier>();
    }
}
