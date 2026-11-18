using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

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

        var button = CustomButtonSingleton<ComposterPlaceTrapButton>.Instance;
        button.SetUses((int)OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound);
        button.ResetCooldownAndOrEffect();
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player.Data.Role is ComposterRole role &&
            !OptionGroupSingleton<ComposterOptions>.Instance.TrapsWorkAfterDeath)
        {
            role.ClearTraps();
        }
    }
}
