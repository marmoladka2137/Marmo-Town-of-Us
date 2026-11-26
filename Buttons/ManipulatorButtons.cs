using MiraAPI.Hud;
using Reactor.Utilities.Extensions;
using TouMarmo.Assets;
using TouMarmo.Modules.Manipulator;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;

public sealed class ManipulateButton : TownOfUsRoleButton<ManipulatorRole>
{
    private Vector3 labelPosition, labelScale;
    private bool labelReady;
    public override string Name => "Manipulate";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUs.TownOfUsColors.Impostor;
    public override float Cooldown => ManipulatorRuntime.Options.Cooldown.Value;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => ManipulatorAssets.Tablet;
    public override bool CanUse() => base.CanUse() && ManipulatorRuntime.CanStart(PlayerControl.LocalPlayer);
    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        labelReady=Button && Button!.buttonLabelText;
        if(!labelReady) return;
        var text=Button!.buttonLabelText.transform;
        labelPosition=text.localPosition;
        labelScale=text.localScale*.82f;
        text.localScale=labelScale;
    }
    protected override void FixedUpdate(PlayerControl p)
    {
        base.FixedUpdate(p);
        if(!labelReady || !Button || !Button!.buttonLabelText) return;
        var text=Button.buttonLabelText.transform;
        text.localScale=labelScale;
        text.localPosition=labelPosition;
    }
    public override void FixedUpdateHandler(PlayerControl p)
    {
        SetTimer(ManipulatorRuntime.Cooldown(p));
        base.FixedUpdateHandler(p);
    }
    protected override void OnClick()
    {
        var menu=CustomPlayerMenu.Create();
        menu.Begin(p => MarmoSession.Alive(p) && p != PlayerControl.LocalPlayer,
            p => { menu.ForceClose(); if(p) ManipulatorRuntime.RequestAfterMenu(p!); });
    }
}

public sealed class ManipulatedKillButton : TownOfUsTargetButton<PlayerControl>, IKillButton
{
    public override string Name => "Forced Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Color.red;
    public override float Cooldown => 0;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public override bool Enabled(RoleBehaviour? role) => ManipulatorRuntime.VictimSession(PlayerControl.LocalPlayer)!=null;
    public override bool CanUse() => base.CanUse() && ManipulatorRuntime.CanAttack(PlayerControl.LocalPlayer);
    // Do not emit a generic local-player visit: framed murder emits the actual
    // Manipulator source for Lookout, Celebrity, Big Brain and kill history.
    public override void ClickHandler() { if(CanClick()) OnClick(); }
    public override PlayerControl? GetTarget() => ManipulatorRuntime.Target(PlayerControl.LocalPlayer);
    public override void SetOutline(bool active)
    {
        if(Target) Target!.cosmetics.currentBodySprite.BodySprite.SetOutline(active ? Color.red : null);
    }
    protected override void OnClick()
    {
        var s=ManipulatorRuntime.VictimSession(PlayerControl.LocalPlayer);
        if(s==null || !Target) return;
        s.RetryAt=Time.unscaledTime+.5f;
        // RetryAt only throttles local clicks; authoritative checks run on the host.
        if(MarmoSession.IsHost) s.RetryAt=0;
        ManipulatorRuntime.RpcAttack(PlayerControl.LocalPlayer,Target!,s.Id);
    }
}
