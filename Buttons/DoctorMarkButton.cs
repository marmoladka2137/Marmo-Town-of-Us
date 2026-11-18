using TouMarmo.Modules;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class DoctorMarkButton : TownOfUsRoleButton<DoctorRole, PlayerControl>
{
    public override string Name => "Mark";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(70,202,220,255);
    public override float Cooldown => DoctorRuntime.Options.Cooldown.Value;
    public override float InitialCooldown => 0;
    public override float Distance => 2.25f;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.Doctor;
    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    public override bool CanUse() => base.CanUse() && Target != null && DoctorRuntime.CanMark(PlayerControl.LocalPlayer, Target);
    protected override void OnClick() { if (Target) DoctorRuntime.RpcMarkRequest(PlayerControl.LocalPlayer, Target!); }
}
