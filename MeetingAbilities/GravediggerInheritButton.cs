using MiraAPI.MeetingAbilities;
using TouMarmo.Modules.Gravedigger;
using UnityEngine;
namespace TouMarmo.MeetingAbilities;

public sealed class GravediggerInheritButton : MeetingActionButton
{
    public override string Name => "Inherit Role";
    public override float Cooldown => .001f;
    public override float InitialCooldown => .001f;
    public override int MaxUses => 0;
    public override bool HideUponWrapUp => true;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.GravediggerAssets.Inherit;
    public override Color TextOutlineColor => GravediggerRole.Color;
    public override bool Enabled(RoleBehaviour? role) => role is GravediggerRole;
    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        TouMarmo.Patches.AbilityVisualSizes.FitGraphic(Button?.graphic);
    }
    public override bool CanUse() => base.CanUse() && GravediggerRuntime.Choosing(PlayerControl.LocalPlayer);
    protected override void OnClick() => GravediggerRuntime.OpenMenu();
}
