// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace TouMarmo.Options;

public sealed class UniversalModifiersOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 1;

    public ModdedNumberOption BalloonAmount { get; } = new("ComposterAddon.Option.BalloonAmount", 1f, 0f, 15f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);


    public ModdedNumberOption BalloonChance { get; } = new("ComposterAddon.Option.BalloonChance", 0f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedNumberOption BalloonDuration { get; } = new("ComposterAddon.Option.BalloonDuration", 3f, 1f, 15f, 0.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);


    public ModdedNumberOption KemadaAmount { get; } = new("TouMarmo.Option.KemadaAmount", 1f, 0f, 15f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);


    public ModdedNumberOption KemadaChance { get; } = new("TouMarmo.Option.KemadaChance", 0f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedToggleOption KemadaGuessable { get; } = new("Kemada Can Be Guessed", true);

    public ModdedNumberOption BigBrainChance { get; } = new("Big Brain Chance", 0f, 0, 100, 5, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedNumberOption BigBrainAmount { get; } = new("Big Brain Maximum Players", 1, 0, 15, 1, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedNumberOption BigBrainDetections { get; } = new("Big Brain Detections Per Round", 3, 1, 15, 1, "#", "#", MiraNumberSuffixes.None, null, halfIncrements: true);

    public ModdedToggleOption BigBrainFailed { get; } = new("Big Brain Detects Failed Abilities", false);
public ModdedToggleOption BigBrainCrew { get; } = new("Big Brain For Crewmates", true);

    public ModdedToggleOption BigBrainImpostor { get; } = new("Big Brain For Impostors", true);

    public ModdedToggleOption BigBrainNeutral { get; } = new("Big Brain For Neutrals", true);

    public ModdedNumberOption OneMoreChanceChance { get; } = new("One More Chance Chance", 0f, 0f, 100f, 5f, "#", "#", MiraNumberSuffixes.Percent, null, halfIncrements: false);
public ModdedNumberOption OneMoreChanceCount { get; } = new("One More Chance Count", 1, 0f, 15f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: true);

}


