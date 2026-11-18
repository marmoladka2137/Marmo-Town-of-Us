// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Crewmate;

namespace TouMarmo.Options;

// Shown inside the Time Lord role expansion (MiraAPI groups options by OptionableType).
public sealed class TimeLordRiskOptions : AbstractRoleOptionGroup<TimeLordRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Option.TimeLordRiskGroup", "Time Lord Rewind Risk");

    public ModdedNumberOption DeathChance { get; } = new("TouMarmo.Option.TimeLordDeathChance", 0f, 0f, 100f, 1f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: true);
}

