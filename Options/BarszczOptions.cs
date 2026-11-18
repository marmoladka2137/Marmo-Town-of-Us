// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TouMarmo.Options;

public sealed class BarszczOptions : AbstractRoleOptionGroup<BarszczRole>
{
    public override string GroupName => MiraLocaleManager.Get("ComposterAddon.Role.Barszcz", "Barszcz");

    public ModdedNumberOption KillCooldown { get; } = new("ComposterAddon.Option.BarszczKillCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedToggleOption CanVent { get; } = new("ComposterAddon.Option.BarszczCanVent", true);


    public ModdedToggleOption RevealBody { get; } = new("ComposterAddon.Option.BarszczRevealBody", true);


    public ModdedToggleOption SwapPositions { get; } = new("ComposterAddon.Option.BarszczSwapPositions", true);

}

