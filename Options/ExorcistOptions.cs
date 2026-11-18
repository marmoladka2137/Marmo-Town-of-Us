// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;

namespace TouMarmo.Options;

public sealed class ExorcistOptions : AbstractRoleOptionGroup<ExorcistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Role.Exorcist", "Exorcist");

    public ModdedNumberOption DragCooldown { get; } = new("TouMarmo.Option.ExorcistDragCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption PrayerCooldown { get; } = new("TouMarmo.Option.ExorcistPrayerCooldown", 25f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption PrayerUses { get; } = new("TouMarmo.Option.ExorcistPrayerUses", 3f, 1f, 10f, 1f, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: true);


    public ModdedNumberOption BodyDetectionRadius { get; } = new("TouMarmo.Option.ExorcistBodyDetectionRadius", 1.5f, 0.5f, 5f, 0.25f, "#", "#", MiraNumberSuffixes.Multiplier, "0.00", halfIncrements: false);

}

