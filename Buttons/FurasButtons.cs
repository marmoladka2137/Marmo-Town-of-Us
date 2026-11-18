using TouMarmo.Assets;
using TouMarmo.Modules;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class FurasPlaceButton : TownOfUsRoleButton<FurasRole>
{
    public override string Name => "Place Spawn";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => new Color32(239, 153, 69, 255);
    public override float InitialCooldown => 0;
    public override float Cooldown => FurasRuntime.Options.PlaceCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => FurasAssets.PlaceSpawn;
    public override bool CanUse() => base.CanUse() && FurasRuntime.Available(PlayerControl.LocalPlayer) && !FurasRuntime.For(PlayerControl.LocalPlayer).Active &&
        FurasRuntime.For(PlayerControl.LocalPlayer).Points.Count < FurasRuntime.Options.MaxPoints.Value;
    protected override void OnClick() => FurasRuntime.RpcPlace(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.GetTruePosition());
}
public sealed class FurasControlButton : TownOfUsRoleButton<FurasRole>
{
    protected override void FixedUpdate(PlayerControl p)
    {
        if (FurasRuntime.Available(p) && FurasRuntime.For(p).Active)
            Button?.gameObject.SetActive(true);
        else
            base.FixedUpdate(p);
    }

    public override string Name => "Control Pet";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(239, 153, 69, 255);
    public override float InitialCooldown => 0;
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => FurasAssets.Pet;
    public override bool CanUse()
    {
        var p = PlayerControl.LocalPlayer;
        if (!p) return false;
        var s = FurasRuntime.For(p);
        return FurasRuntime.Available(p) && (s.Active || base.CanUse() && FurasRuntime.CanDrive(p) && s.Points.Count > 0 && Time.time >= s.ReadyAt);
    }
    public override void FixedUpdateHandler(PlayerControl p)
    {
        var s = FurasRuntime.For(p);
        SetTimer(0);
        base.FixedUpdateHandler(p);
        OverrideName(s.Active ? "RETURN" : s.Lost ? "ABILITY LOST" : "CONTROL PET");
        if (!Button) return;
        var remaining = Mathf.Max(0,(s.Active ? s.EndsAt : s.ReadyAt)-Time.time);
        Button!.SetCooldownFormat(remaining,s.Active ? FurasRuntime.Options.ControlDuration.Value : FurasRuntime.Options.ControlCooldown.Value,CooldownTimerFormatString);
        var timer = Button!.cooldownTimerText;
        var standard=HudManager.Instance.KillButton.cooldownTimerText;
        timer.transform.localPosition=standard.transform.localPosition;
        timer.transform.localScale=standard.transform.localScale;
        timer.enableAutoSizing=false;
        timer.fontSize=standard.fontSize;
        timer.enableWordWrapping = false;
        timer.rectTransform.sizeDelta = new Vector2(2f,1f);
    }

    protected override void OnClick()
    {
        var p = PlayerControl.LocalPlayer; var s = FurasRuntime.For(p);
        if (s.Active) FurasRuntime.RpcStop(p, s.ControlId); else FurasRuntime.BeginControl(p, s.Selected);
    }
}
public sealed class FurasNextSpawnButton : TownOfUsRoleButton<FurasRole>
{
    public override string Name => "Spawn";
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => new Color32(239, 153, 69, 255);
    public override float InitialCooldown => 0;
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => FurasAssets.NextSpawn;
    public override bool CanUse() => base.CanUse() && FurasRuntime.Available(PlayerControl.LocalPlayer) && !FurasRuntime.For(PlayerControl.LocalPlayer).Active && FurasRuntime.For(PlayerControl.LocalPlayer).Points.Count > 1;
    public override void FixedUpdateHandler(PlayerControl p)
    {
        base.FixedUpdateHandler(p);
        var s = FurasRuntime.For(p);
        OverrideName(s.Points.Count == 0 ? "NO SPAWN" : $"SPAWN {s.Selected + 1}/{s.Points.Count}");
    }
    protected override void OnClick()
    {
        var s = FurasRuntime.For(PlayerControl.LocalPlayer);
        s.Selected = (s.Selected + 1) % s.Points.Count;
    }
}
public sealed class FurasAttackButton : TownOfUsRoleButton<FurasRole>
{
    public override void FixedUpdateHandler(PlayerControl p)
    {
        var state = FurasRuntime.For(p);
        // Use the same deadline as the host instead of retaining a previous control's timer.
        SetTimer(state.Active ? Mathf.Max(0, state.KillReadyAt - Time.time) : 0);
        base.FixedUpdateHandler(p);
        if (Button && state.Active)
            Button!.SetCooldownFormat(Mathf.Max(0, state.KillReadyAt - Time.time), Mathf.Max(0.01f, FurasRuntime.Options.KillCooldown.Value), CooldownTimerFormatString);
    }

    protected override void FixedUpdate(PlayerControl p)
    {
        if (FurasRuntime.Available(p) && FurasRuntime.For(p).Active)
            Button?.gameObject.SetActive(true);
        else
            base.FixedUpdate(p);
    }

    public override string Name => "Pet Kill";
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => new Color32(239, 153, 69, 255);
    public override float InitialCooldown => 0;
    public override float Cooldown => FurasRuntime.Options.KillCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => FurasAssets.PetAttack;
    public override bool CanUse()
    {
        var p = PlayerControl.LocalPlayer;
        if (!p) return false;
        var s = FurasRuntime.For(p);
        return FurasRuntime.CanDrive(p) && s.Active && !s.AttackPending && Time.unscaledTime >= s.RequestUntil && Time.time >= s.KillReadyAt && s.Kills < FurasRuntime.Options.MaxKills.Value && FurasRuntime.Target(p) != null;
    }
    protected override void OnClick()
    {
        var p = PlayerControl.LocalPlayer; var target = FurasRuntime.Target(p);
        if (target == null) return;
        FurasRuntime.RequestAttack(p, target);
    }
}

