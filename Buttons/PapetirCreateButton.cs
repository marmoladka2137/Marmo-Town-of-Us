using TouMarmo.Modules.Papetir;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class PapetirCreateButton : TownOfUsRoleButton<PapetirRole>
{
    public override string Name=>"Create Body";
    public override BaseKeybind Keybind=>Keybinds.SecondaryAction;
    public override Color TextOutlineColor=>PapetirRole.Color;
    public override LoadableAsset<Sprite> Sprite=>PapetirAssets.Paper;
    public override float Cooldown=>PapetirRuntime.Options.CreateCooldown.Value;
    public override float InitialCooldown=>0;
    public override bool ZeroIsInfinite {get;set;}=true;
    public override int MaxUses=>(int)PapetirRuntime.Options.BodyLimit.Value;
    public override bool CanUse()=>base.CanUse() && PapetirRuntime.CanCreate(PlayerControl.LocalPlayer);
    public override void FixedUpdateHandler(PlayerControl p)
    {
        SetTimer(PapetirRuntime.Remaining(p));
        SetUses(MaxUses<=0 ? 0 : Math.Max(0,MaxUses-PapetirRuntime.Created(p)));
        base.FixedUpdateHandler(p);
        if(Button && MaxUses<=0) Button!.SetInfiniteUses();
    }
    public override void ClickHandler(){if(CanClick()) PapetirRuntime.RpcCreate(PlayerControl.LocalPlayer);}
    protected override void OnClick(){}
}
