using TouMarmo.Assets;
using TouMarmo.Options;
using MiraAPI.GameOptions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class BalloonModifier : UniversalGameModifier
{
    public override string IdPart => "Balloon";
    public override string ModifierNameLocale => "ComposterAddon.Modifier.Balloon";
    public override string ModifierMedDescriptionLocale => "ComposterAddon.Modifier.Balloon.MedDescription";
    public override string ModifierWikiDescriptionLocale => "ComposterAddon.Modifier.Balloon.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => BalloonAssets.Icon;
    public override ModifierUiConfiguration Configuration => new(
        new Color32(246, 55, 70, 255),
        TmpSpriteUtils.CreateSpriteAsset(BalloonAssets.Icon.LoadAsset(), "ComposterAddon.Modifier.Balloon", 1.45f));
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(246, 55, 70, 255);

    public override string GetDescription() => MiraLocaleManager.Get(
        "ComposterAddon.Modifier.Balloon.TabDescription",
        "Players who interact with you receive a balloon above their head.");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.BalloonChance.Value;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.BalloonAmount.Value;
}
