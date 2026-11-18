using TouMarmo.Modules;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class BanditStealButton : TownOfUsRoleButton<BanditRole,PlayerControl>
{
    public override string Name=>"Steal Role";
    public override BaseKeybind Keybind=>Keybinds.PrimaryAction;
    public override Color TextOutlineColor=>new Color32(135,105,75,255);
    public override float Cooldown=>BanditRuntime.Options.Cooldown.Value;
    public override float InitialCooldown=>0;
    public override float Distance=>BanditRuntime.StealRange;
    public override LoadableAsset<Sprite> Sprite=>TouMarmo.Assets.MarmoAssets.Bandit;
    public override PlayerControl? GetTarget()=>PlayerControl.LocalPlayer.GetClosestLivingPlayer(true,Distance, predicate: p => BanditRuntime.CanSteal(PlayerControl.LocalPlayer,p));
    public override bool CanUse()=>base.CanUse() && Target && BanditRuntime.CanSteal(PlayerControl.LocalPlayer,Target!);
    protected override void OnClick(){if(Target) BanditRuntime.RpcStealRequest(PlayerControl.LocalPlayer,Target!);}
}
