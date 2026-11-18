// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using UnityEngine;
namespace TouMarmo.Options;
public sealed class ImpostorModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 2;
    public override Color GroupColor => new Color32(220, 55, 75, 255);
    public ModdedNumberOption SimpAmount { get; } = new("Simp Amount", 1f, 0f, 15f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);

    public ModdedNumberOption SimpChance { get; } = new("Simp Chance", 0f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedNumberOption SimpPenalty { get; } = new("Simp Cooldown After Target Ejection", 1.25f, 1.05f, 3f, 0.05f, "#", "#", MiraNumberSuffixes.Multiplier, null, halfIncrements: false);

}

