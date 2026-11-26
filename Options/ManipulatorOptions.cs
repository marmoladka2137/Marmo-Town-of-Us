using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;
namespace TouMarmo.Options;
public sealed class ManipulatorOptions : AbstractRoleOptionGroup<ManipulatorRole>
{
    public override string GroupName => "Manipulator";
    public ModdedNumberOption Cooldown { get; } = new("TouMarmo.Option.ManipulateCooldown", 30f, 5f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds);
    public ModdedNumberOption TimeToKill { get; } = new("TouMarmo.Option.ManipulatorTimeToKill", 20f, 5f, 90f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds);
    public ModdedToggleOption MeetingCancels { get; } = new("TouMarmo.Option.ManipulatorMeetingCancels", true);
}
