// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TouMarmo.Options;

public sealed class ComposterOptions : AbstractRoleOptionGroup<ComposterRole>
{
    public override string GroupName => MiraLocaleManager.Get("ComposterAddon.Role.Composter", "Composter");

    public ModdedNumberOption TrapCooldown { get; } = new("ComposterAddon.Option.TrapCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption MaxTrapsPerRound { get; } = new("ComposterAddon.Option.MaxTrapsPerRound", 2f, 1f, 10f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);


    public ModdedNumberOption TrapRadius { get; } = new("ComposterAddon.Option.TrapRadius", 0.3f, 0.1f, 0.75f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00", halfIncrements: false);


    public ModdedNumberOption ArmingDelay { get; } = new("ComposterAddon.Option.ArmingDelay", 1.5f, 0f, 10f, 0.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedToggleOption ShowOwnerMarker { get; } = new("ComposterAddon.Option.ShowOwnerMarker", true);


    public ModdedToggleOption WorkAfterDeath { get; } = new("ComposterAddon.Option.WorkAfterDeath", false);
}

