using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class ComposterOptions : AbstractRoleOptionGroup<ComposterRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Composter", "Composter");

    [ModdedNumberOption("TouOptionComposterTrapCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TrapCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionComposterMaxTrapsPerRound", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxTrapsPerRound { get; set; } = 2f;

    [ModdedNumberOption("TouOptionComposterTrapRadius", 0.1f, 0.75f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float TrapRadius { get; set; } = 0.3f;

    [ModdedNumberOption("TouOptionComposterArmingDelay", 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ArmingDelay { get; set; } = 1.5f;

    [ModdedToggleOption("TouOptionComposterSeesTrapMarkers")]
    public bool SeesTrapMarkers { get; set; } = true;

    [ModdedToggleOption("TouOptionComposterTrapsWorkAfterDeath")]
    public bool TrapsWorkAfterDeath { get; set; } = false;
}
