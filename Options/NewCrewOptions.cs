// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;
namespace TouMarmo.Options;
public sealed class BanditOptions : AbstractRoleOptionGroup<BanditRole>
{
    public override string GroupName => "Bandit";
    public ModdedNumberOption Cooldown { get; } = new("Steal Cooldown", 25, 5, 120, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption MaxSteals { get; } = new("Maximum Steals Per Game", 3, 0, 10, 1, "∞", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedToggleOption Crew { get; } = new("Can Steal Crewmate Roles", true);

    public ModdedToggleOption Impostor { get; } = new("Can Steal Impostor Roles", true);

    public ModdedToggleOption Neutral { get; } = new("Can Steal Neutral Roles", true);

}
public sealed class DoctorOptions : AbstractRoleOptionGroup<DoctorRole>
{
    public override string GroupName => "Doctor";
    public ModdedNumberOption Cooldown { get; } = new("Mark Cooldown", 15, 1, 60, 1, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: true);

    public ModdedNumberOption Percent { get; } = new("Required Living Crewmates Marked", 50, 25, 100, 5, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);

    public ModdedToggleOption Immortal { get; } = new("Invulnerable After Reveal", true);

    public ModdedToggleOption CanBeExiled { get; } = new("Can Be Voted Out After Reveal", true);

    public ModdedNumberOption MarksPerRound { get; } = new("Marks Per Round", 3, 0, 15, 1, "∞", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

}
public sealed class NecromancerOptions : AbstractRoleOptionGroup<NecromancerRole>
{
    public override string GroupName => "Necromancer";
    public ModdedNumberOption Cooldown { get; } = new("Corpse Ability Cooldown", 25, 5, 90, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption ControlUses { get; } = new("Control Uses Per Game", 3, 0, 10, 1, "∞", "#", MiraNumberSuffixes.None, null, halfIncrements: true);


    public ModdedNumberOption ControlDuration { get; } = new("Corpse Control Duration", 20, 5, 90, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption KillCooldown { get; } = new("Corpse Kill Cooldown", 3, 0, 30, 1, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: true);
    public ModdedNumberOption KillsPerControl { get; } = new("Corpse Kills Per Control", 1, 0, 10, 1, "∞", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);

    public ModdedNumberOption Speed { get; } = new("Corpse Movement Speed", 1.5f, 0.5f, 4, 0.25f, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: false);


public ModdedToggleOption CanReport { get; } = new("Moving Corpses Can Be Reported", true);

    public ModdedToggleOption InterruptOnMeeting { get; } = new("End Corpse Abilities On Meeting", true);

}

