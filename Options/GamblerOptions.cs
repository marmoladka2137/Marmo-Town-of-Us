// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;

namespace TouMarmo.Options;

public sealed class GamblerOptions : AbstractRoleOptionGroup<GamblerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Role.Gambler", "Gambler");

    public ModdedNumberOption MaxMarks { get; } = new("TouMarmo.Option.MaxMarks", 3f, 0, 5f, 1f, "∞", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);


    public ModdedNumberOption ShootCooldown { get; } = new("TouMarmo.Option.ShootCooldown", 15f, 5f, 120f, 5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

}

