// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TouMarmo.Options;

public sealed class HuggerOptions : AbstractRoleOptionGroup<HuggerRole>
{
    public override string GroupName => MiraLocaleManager.Get("ComposterAddon.Role.Hugger", "Hugger");

    public ModdedNumberOption HugCooldown { get; } = new("ComposterAddon.Option.HugCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption BodyDisappearChance { get; } = new("ComposterAddon.Option.BodyDisappearChance", 30f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);


    public ModdedToggleOption CanVent { get; } = new("ComposterAddon.Option.HuggerCanVent", true);

}

