// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;

namespace TouMarmo.Options;

public sealed class WladcaOptions : AbstractRoleOptionGroup<WladcaRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Role.Wladca", "Wladca");

    public ModdedNumberOption Cooldown { get; } = new("TouMarmo.Option.WladcaCooldown", 40f, 5f, 120f, 5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption Duration { get; } = new("TouMarmo.Option.WladcaDuration", 30f, 5f, 120f, 5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption Uses { get; } = new("TouMarmo.Option.WladcaUses", 0f, 0f, 10f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);
public ModdedToggleOption ImpostorVision { get; } = new("TouMarmo.Option.WladcaImpostorVision", true);


    public ModdedToggleOption IgnoreComms { get; } = new("TouMarmo.Option.WladcaIgnoreComms", true);


    public ModdedToggleOption SeeInLights { get; } = new("TouMarmo.Option.WladcaSeeInLights", true);

}

