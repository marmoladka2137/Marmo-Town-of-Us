using MiraAPI.GameOptions;
using TouMarmo.Modules.Hunger;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class HungerModifier : TouGameModifier, TownOfUs.Extensions.IUnguessableBasic
{
    public HungerProgress Progress { get; } = new();
    public bool IsGuessable => false;
    public override bool HideFromGuessing => true;
    public override bool PreventsOtherModifiers => false;
    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "Hunger";
    public override string ModifierNameLocale => "TouMarmo.Modifier.Hunger";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.Hunger.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.Hunger.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => HungerAssets.Icon;
    public override ModifierUiConfiguration Configuration => new(
        new Color32(225, 124, 43, 255),
        TmpSpriteUtils.CreateSpriteAsset(HungerAssets.Icon.LoadAsset(), "TouMarmo.Modifier.Hunger", 1.45f));
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(225, 124, 43, 255);
    public override bool IsModifierValidOn(RoleBehaviour role) => role is not SpectatorRole;
    public override string GetDescription() => MiraLocaleManager.Get(
        "TouMarmo.Modifier.Hunger.TabDescription", "Report bodies to move faster.");
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.HungerChance.Value;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.HungerAmount.Value;
}
