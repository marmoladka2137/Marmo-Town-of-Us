using MiraAPI.GameOptions;
using TouMarmo.Assets;
using TouMarmo.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class KemadaModifier : TouGameModifier, TownOfUs.Extensions.IUnguessableBasic
{
    public bool IsGuessable => OptionGroupSingleton<UniversalModifiersOptions>.Instance.KemadaGuessable.Value;
    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "Kemada";
    public override string ModifierNameLocale => "TouMarmo.Modifier.Kemada";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.Kemada.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.Kemada.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => MarmoAssets.Kemada;
    public override ModifierUiConfiguration Configuration => new(
        new Color32(151, 44, 202, 255),
        TmpSpriteUtils.CreateSpriteAsset(MarmoAssets.Kemada.LoadAsset(), "TouMarmo.Modifier.Kemada", 1.45f));
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(151, 44, 202, 255);
    public override bool PreventsOtherModifiers => false;
    public override bool IsModifierValidOn(RoleBehaviour role) => role is not SpectatorRole;
    public override string GetDescription() => MiraLocaleManager.Get("TouMarmo.Modifier.Kemada.TabDescription", "Your vote goes to a random living player.");
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.KemadaChance.Value;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.KemadaAmount.Value;
}
