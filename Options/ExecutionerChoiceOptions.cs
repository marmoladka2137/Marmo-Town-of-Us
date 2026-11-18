// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Neutral;

namespace TouMarmo.Options;

// Shown inside the Executioner role expansion (MiraAPI groups options by OptionableType).
public sealed class ExecutionerChoiceOptions : AbstractRoleOptionGroup<ExecutionerRole>
{
    public override string GroupName => "Executioner - First Round Choice";

    public override uint GroupPriority => 0;

    public ModdedToggleOption ChangeTarget { get; } = new("Allow Target Change In First Round", false);


    public ModdedToggleOption Renounce { get; } = new("Allow Renouncing To Amnesiac In First Round", false);

}

