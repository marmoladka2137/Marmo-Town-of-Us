// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TouMarmo.Options;

public sealed class SweetsOptions : AbstractRoleOptionGroup<SweetsRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Role.Sweets", "Sweets");

    public ModdedNumberOption FlowerCooldown { get; } = new("TouMarmo.Option.SweetsFlowerCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    // Number of flowers handed out before the ability button becomes Pollinate.
    public ModdedNumberOption MaxFlowers { get; } = new("TouMarmo.Option.SweetsMaxFlowers", 3f, 1f, 10f, 1f, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: true);


    public ModdedNumberOption PollinatedKillChance { get; } = new("TouMarmo.Option.SweetsKillChance", 40f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);

}

