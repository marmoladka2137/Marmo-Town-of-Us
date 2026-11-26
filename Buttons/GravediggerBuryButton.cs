using TouMarmo.Modules.Gravedigger;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;

public sealed class GravediggerBuryButton : TownOfUsRoleButton<GravediggerRole, DeadBody>
{
    private float lastRequest = -10;
    public override string Name => "Bury";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => GravediggerRuntime.Options.BuryCooldown.Value;
    public override float InitialCooldown => 0;
    public override float Distance => 1.5f;
    public override Color TextOutlineColor => GravediggerRole.Color;
    public override LoadableAsset<Sprite> Sprite => GravediggerAssets.Bury;
    public override DeadBody? GetTarget() => PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    public override bool CanUse() => base.CanUse() && GravediggerRuntime.CanBury(PlayerControl.LocalPlayer, Target);
    public override void ClickHandler()
    {
        if (!CanClick() || Time.unscaledTime - lastRequest < .25f) return;
        lastRequest = Time.unscaledTime;
        OnClick(); // Host acceptance, rather than a local failed click, starts the cooldown.
    }
    public override void FixedUpdateHandler(PlayerControl p)
    {
        SetTimer(GravediggerRuntime.Remaining(p));
        base.FixedUpdateHandler(p);
        if (Button)
        {
            Button!.usesRemainingSprite.gameObject.SetActive(false);
            Button.usesRemainingText.gameObject.SetActive(false);
        }
    }
    protected override void OnClick()
    {
        if (Target) GravediggerRuntime.RpcBuryRequest(PlayerControl.LocalPlayer, Target!.ParentId);
    }
}
