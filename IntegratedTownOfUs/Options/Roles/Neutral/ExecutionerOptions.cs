using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ExecutionerOptions : AbstractRoleOptionGroup<ExecutionerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Executioner", "Executioner");

    [ModdedEnumOption("TouOptionExecutionerBecomesTargetDeath", typeof(BecomeOptions), ["MiraApi.RoleTeam.Crewmate", "TownOfUsMira.Role.Amnesiac", "TownOfUsMira.Role.Survivor", "TownOfUsMira.Role.Mercenary", "TownOfUsMira.Role.Jester"])]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Jester;

    [ModdedToggleOption("TouOptionExecutionerCanButton")]
    public bool CanButton { get; set; } = true;

    private ExeWinOptions winMode = ExeWinOptions.Nothing;
    [ModdedEnumOption("TouOptionExecutionerWin", typeof(ExeWinOptions), ["TouOptionExecutionerWinEnumEndsGame", "TouOptionExecutionerWinEnumNothing", "TouOptionExecutionerWinEnumNothing"])]
    public ExeWinOptions ExeWin
    {
        // Preserve old saved enum values without re-enabling the removed revenge ability.
        get => winMode == ExeWinOptions.Torments ? ExeWinOptions.Nothing : winMode;
        set => winMode = value == ExeWinOptions.Torments ? ExeWinOptions.Nothing : value;
    }

    public ModdedToggleOption ExeAnonymizeWin { get; set; } =
        new("TouOptionNeutAnonymousVictoryWin", false)
    {
        Visible = () => OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is not ExeWinOptions.EndsGame
    };
}

public enum ExeWinOptions
{
    EndsGame,
    Torments,
    Nothing
}
