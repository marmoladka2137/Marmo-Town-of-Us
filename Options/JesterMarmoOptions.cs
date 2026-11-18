// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Neutral;

namespace TouMarmo.Options;

// Shown inside the Jester role expansion (MiraAPI groups options by OptionableType).
public sealed class JesterMarmoOptions : AbstractRoleOptionGroup<JesterRole>
{
    public override string GroupName => "Jester - First Body Arrow";

    public override uint GroupPriority => 0;

    public ModdedToggleOption FirstBodyArrow { get; } = new("Arrow To First Body In Round", true);

}

