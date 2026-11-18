// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
namespace TouMarmo.Options;
public sealed class MeetingGuessingOptions : AbstractOptionGroup
{
    public override string GroupName => "Meeting Guessing";
    public override MenuCategory ParentMenu => MenuCategory.Game;
    public ModdedToggleOption PestilenceGuessable { get; } = new("Pestilence Can Be Guessed", false);
}

