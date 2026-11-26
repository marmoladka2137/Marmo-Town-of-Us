// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;
namespace TouMarmo.Options;
public sealed class FurasOptions : AbstractRoleOptionGroup<FurasRole>
{
    public override string GroupName => "Furas";
    public ModdedNumberOption MaxPoints { get; } = new("Maximum Spawn Points", 3, 0, 6, 1, "∞", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedNumberOption PlaceCooldown { get; } = new("Place Spawn Cooldown", 5, 1, 30, 1, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: true);

    public ModdedNumberOption ControlCooldown { get; } = new("Pet Control Cooldown", 25, 5, 90, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption ControlDuration { get; } = new("Pet Control Duration", 20, 5, 60, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption SpeedMultiplier { get; } = new("Pet Speed Multiplier", 1.25f, 0.5f, 2f, 0.05f, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: false);

    public ModdedNumberOption KillCooldown { get; } = new("Pet Kill Cooldown", 15, 5, 60, 5, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);

    public ModdedNumberOption FirstKillDelay { get; } = new("Pet First Kill Delay", 3, 0, 15, 1, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: true);

    public ModdedNumberOption KillRange { get; } = new("Pet Kill Range", 1, 0.5f, 2f, 0.25f, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: false);

    public ModdedNumberOption MaxKills { get; } = new("Kills Per Control", 1, 0, 5, 1, "∞", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedToggleOption LoseOnCrewKill { get; } = new("Lose Ability After Killing A Crewmate", true);

    public ModdedToggleOption ReturnAfterKill { get; } = new("Return After Pet Kill", false);

    public ModdedToggleOption KeepPoints { get; } = new("Keep Spawn Points After Meetings", true);

    public ModdedToggleOption VisiblePet { get; } = new("Other Players Can See Pet", true);

    public ModdedToggleOption ShowMarkers { get; } = new("Show Own Spawn Markers", true);

    public ModdedToggleOption BlockedByComms { get; } = new("Comms Blocks Pet Control", false) { Visible = () => false }; // Retain option ID for existing presets.

}

