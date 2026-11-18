using MiraAPI.GameOptions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TouMarmo.Options;
using UnityEngine;
namespace TouMarmo.Modifiers;
public sealed class SimpModifier : TouGameModifier
{
    public byte TargetId { get; set; } = byte.MaxValue;
    public bool TargetEjected { get; set; }
    public PlayerControl? Target => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == TargetId);
    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "Simp";
    public override string ModifierNameLocale => "TouMarmo.Modifier.Simp";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.Simp.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.Simp.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => TouMarmo.Assets.MarmoAssets.Simp;
    public override ModifierUiConfiguration Configuration => new(new Color32(235, 90, 135, 255),
        TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Simp.LoadAsset(), "TouMarmo.Modifier.Simp", 1.45f));
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => new Color32(235, 90, 135, 255);
    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && role.IsImpostor();
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<ImpostorModifiersOptions>.Instance.SimpAmount.Value;
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ImpostorModifiersOptions>.Instance.SimpChance.Value;
    public override string GetDescription()
    {
        if (TargetId == byte.MaxValue) return "Waiting for a target.";
        return TargetEjected ? "Target ejected. Kill cooldown increased." :
            $"Protect {Target?.Data?.PlayerName ?? "(disconnected)"}. Kill cooldown -25%.";
    }
}
