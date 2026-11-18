// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
namespace TouMarmo.Options;
public sealed class AssailantModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => "Assailant Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;
    public ModdedNumberOption Chance { get; } = new("Triple Shot Chance", 0f, 0, 100, 5, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedNumberOption Amount { get; } = new("Triple Shot Maximum Players", 1, 0, 15, 1, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedToggleOption Impostors { get; } = new("Triple Shot For Impostors", true);

    public ModdedToggleOption Neutrals { get; } = new("Triple Shot For Neutrals", true);

}

