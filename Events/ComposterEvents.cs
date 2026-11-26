using TouMarmo.Buttons;
using TouMarmo.Options;
using TouMarmo.Roles;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;

namespace TouMarmo.Events;

public static class ComposterEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        CustomRoleUtils.GetActiveRolesOfType<ComposterRole>().Do(x => x.ResetRound());

        if (PlayerControl.LocalPlayer.Data.Role is not ComposterRole)
        {
            return;
        }

        var button = CustomButtonSingleton<PlaceTrapButton>.Instance;
        button.SetUses((int)TouMarmo.Modules.AbilityLimits.Count(TouMarmo.Modules.AbilityLimits.Count(OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound.Value)));
        button.ResetCooldownAndOrEffect();
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player.Data.Role is ComposterRole role &&
            !OptionGroupSingleton<ComposterOptions>.Instance.WorkAfterDeath.Value)
        {
            role.ClearTraps();
        }
    }
}
