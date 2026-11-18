using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Modules.Plumber;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberNailButton : TownOfUsVentRoleButton<PlumberRole>, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.PlumberNail", "Nail");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PlumberOptions>.Instance.TrapCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<PlumberOptions>.Instance.MaxTrappedVents;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.PlumberNail;
    public int ExtraUses { get; set; }
    private float requestedAt = -10f;

    // Uses and cooldown start on the authoritative success receipt, not on a rejected click.
    public override void ClickHandler()
    {
        if (!CanClick() || Time.unscaledTime - requestedAt < .4f) return;
        requestedAt = Time.unscaledTime;
        OnClick();
    }

    public override Vent? GetTarget() => ShipStatus.Instance
        ? ShipStatus.Instance.AllVents.Where(v => v && IsTargetValid(v))
            .OrderBy(v => Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), v.transform.position)).FirstOrDefault()
        : null;

    public override bool CanUse() => !PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.CanMove &&
        TownOfUs.Events.TownOfUsEventHandlers.AreVentsAllowed && base.CanUse();

    public override bool IsTargetValid(Vent? target)
    {
        if (target == null) return false;
        if (!base.IsTargetValid(target)) return false;
        // Can't place nail in a vent that already has one
        if (PlumberNailRuntime.IsTrapped(target.Id)) return false;
        // Can't place more than max trapped vents
        return PlumberNailRuntime.State.CanPlace(target.Id, PlayerControl.LocalPlayer.PlayerId, MaxUses) &&
            PlumberNailRuntime.InRange(PlayerControl.LocalPlayer, target);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error($"{Name}: Target is null");
            return;
        }

        PlumberNailRuntime.RpcPlaceNail(PlayerControl.LocalPlayer, Target.Id);
    }
}

