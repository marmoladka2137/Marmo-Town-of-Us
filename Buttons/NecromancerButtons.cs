using TouMarmo.Modules;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class NecromancerControlButton : TownOfUsRoleButton<NecromancerRole,DeadBody>
{
    public override void FixedUpdateHandler(PlayerControl p)
    {
        var state=NecromancerRuntime.For(p);
        SetTimer(Mathf.Max(0,state.ReadyAt-Time.time));
        base.FixedUpdateHandler(p);
        if (!Button) return;
        Button!.cooldownTimerText.enableWordWrapping=false;
        Button.cooldownTimerText.rectTransform.sizeDelta=new Vector2(2f,1f);
        Button.SetCooldownFormat(Mathf.Max(0,state.ReadyAt-Time.time),Mathf.Max(0.01f,Cooldown),CooldownTimerFormatString);
        Button.SetUsesRemaining(Mathf.Max(0,(int)NecromancerRuntime.Options.ControlUses.Value-state.Controls));
    }

    public override string Name=>"Control Corpse";
    public override BaseKeybind Keybind=>Keybinds.PrimaryAction;
    public override Color TextOutlineColor=>new Color32(119,76,157,255);
    public override float Cooldown=>NecromancerRuntime.Options.Cooldown.Value;
    public override float InitialCooldown=>0;
    public override float Distance=>2.25f;
    public override LoadableAsset<Sprite> Sprite=>TouMarmo.Assets.MarmoAssets.Necromancer;
    public override DeadBody? GetTarget()=>NecromancerRuntime.Nearest(PlayerControl.LocalPlayer,false);
    public override bool CanUse()=>base.CanUse() && Target && NecromancerRuntime.CanUse(PlayerControl.LocalPlayer,Target!,false);
    protected override void OnClick(){if(Target) NecromancerRuntime.RpcRequest(PlayerControl.LocalPlayer,Target!.ParentId,false);}
}
public sealed class NecromancerReanimateButton : TownOfUsRoleButton<NecromancerRole,DeadBody>
{
    public override void FixedUpdateHandler(PlayerControl p)
    {
        var state=NecromancerRuntime.For(p);
        SetTimer(Mathf.Max(0,state.ReadyAt-Time.time));
        base.FixedUpdateHandler(p);
        if (!Button) return;
        Button!.cooldownTimerText.enableWordWrapping=false;
        Button.cooldownTimerText.rectTransform.sizeDelta=new Vector2(2f,1f);
        Button.SetCooldownFormat(Mathf.Max(0,state.ReadyAt-Time.time),Mathf.Max(0.01f,Cooldown),CooldownTimerFormatString);
        Button.SetUsesRemaining(Mathf.Max(0,(int)NecromancerRuntime.Options.ReanimateUses.Value-state.Reanimations));
    }

    public override string Name=>"Reanimate";
    public override BaseKeybind Keybind=>Keybinds.SecondaryAction;
    public override Color TextOutlineColor=>new Color32(119,76,157,255);
    public override float Cooldown=>NecromancerRuntime.Options.Cooldown.Value;
    public override float InitialCooldown=>0;
    public override float Distance=>2.25f;
    public override LoadableAsset<Sprite> Sprite=>TouMarmo.Assets.MarmoAssets.Reanimate;
    public override DeadBody? GetTarget()=>NecromancerRuntime.Nearest(PlayerControl.LocalPlayer,true);
    public override bool CanUse()=>base.CanUse() && Target && NecromancerRuntime.CanUse(PlayerControl.LocalPlayer,Target!,true);
    protected override void OnClick(){if(Target) NecromancerRuntime.RpcRequest(PlayerControl.LocalPlayer,Target!.ParentId,true);}
}
public sealed class NecromancerCorpseKillButton : TownOfUsRoleButton<NecromancerRole>
{
    public override void FixedUpdateHandler(PlayerControl p)
    {
        var state = NecromancerRuntime.For(p);
        // Use the same deadline as the host instead of retaining a previous control's timer.
        SetTimer(state.Active && !state.Killed ? Mathf.Max(0, state.KillAt - Time.time) : 0);
        base.FixedUpdateHandler(p);
        if (Button && state.Active)
        {
            Button!.SetCooldownFormat(state.Killed ? 0 : Mathf.Max(0, state.KillAt - Time.time), Mathf.Max(0.01f, NecromancerRuntime.Options.KillCooldown.Value), CooldownTimerFormatString);
            Button.SetUsesRemaining(state.Killed ? 0 : 1);
        }
    }

    protected override void FixedUpdate(PlayerControl p)
    {
        if (MarmoSession.InRound && NecromancerRuntime.For(p).Active && !NecromancerRuntime.For(p).Paused)
            Button?.gameObject.SetActive(true);
        else
            base.FixedUpdate(p);
    }

    public override string Name=>"Corpse Kill";
    public override BaseKeybind Keybind=>Keybinds.ModifierAction;
    public override Color TextOutlineColor=>new Color32(119,76,157,255);
    public override float InitialCooldown=>0;
    public override float Cooldown=>NecromancerRuntime.Options.KillCooldown.Value;
    public override LoadableAsset<Sprite> Sprite=>TouMarmo.Assets.MarmoAssets.CorpseKill;
    public override bool CanUse()
    {
        var p=PlayerControl.LocalPlayer; if(!MarmoSession.Alive(p) || !MarmoSession.InRound) return false;
        var s=NecromancerRuntime.For(p); return s.Active && !s.Paused && !s.Automatic && !s.Killed && !s.AttackPending && Time.time>=s.KillAt && NecromancerRuntime.Target(p)!=null;
    }
    protected override void OnClick(){var p=PlayerControl.LocalPlayer;var target=NecromancerRuntime.Target(p);if(target) { NecromancerRuntime.RpcMove(p,NecromancerRuntime.For(p).Position); NecromancerRuntime.RpcAttack(p,target!); }}
}
public sealed class NecromancerReturnButton : TownOfUsRoleButton<NecromancerRole>
{
    protected override void FixedUpdate(PlayerControl p)
    {
        if (MarmoSession.InRound && NecromancerRuntime.For(p).Active && !NecromancerRuntime.For(p).Paused)
            Button?.gameObject.SetActive(true);
        else
            base.FixedUpdate(p);
    }

    public override string Name=>"Release Corpse";
    public override BaseKeybind Keybind=>Keybinds.VentAction;
    public override Color TextOutlineColor=>new Color32(119,76,157,255);
    public override float InitialCooldown=>0;
    public override float Cooldown=>0;
    public override LoadableAsset<Sprite> Sprite=>TouMarmo.Assets.MarmoAssets.ReleaseCorpse;
    public override bool CanUse()=>MarmoSession.Alive(PlayerControl.LocalPlayer) && MarmoSession.InRound && NecromancerRuntime.For(PlayerControl.LocalPlayer).Active;
    protected override void OnClick()=>NecromancerRuntime.RpcStop(PlayerControl.LocalPlayer);
}
