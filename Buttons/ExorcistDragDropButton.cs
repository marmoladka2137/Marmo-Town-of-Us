using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TouMarmo.Modifiers;
using TouMarmo.Modules.Exorcist;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class ExorcistDragDropButton : TownOfUsRoleButton<ExorcistRole, DeadBody>
{
    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.ExorcistDrag", "Drag");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => new Color32(177, 156, 217, 255);

    public override float Cooldown => Mathf.Clamp(ExorcistRuntime.Options.DragCooldown.Value + MapCooldown, 5f, 120f);

    public override LoadableAsset<Sprite> Sprite => TouImpAssets.DragSprite;

    public override bool ZeroIsInfinite { get; set; } = true;
    private bool _isProcessingClick;

    public override void ClickHandler()
    {
        if (_isProcessingClick) return;
        _isProcessingClick = true;
        try
        {
            if (!CanClick()) return;
            if (LimitedUses)
            {
                UsesLeft--;
                Button?.SetUsesRemaining(UsesLeft);
            }
            OnClick();
            Button?.SetDisabled();
        }
        finally
        {
            Coroutines.Start(ResetProcessingFlag());
        }
    }

    private System.Collections.IEnumerator ResetProcessingFlag()
    {
        yield return new WaitForSeconds(0.2f);
        _isProcessingClick = false;
    }

    public override DeadBody? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (local && local.TryGetModifier<ExorcistDragModifier>(out var drag)) return drag.DeadBody;
        return local?.GetNearestDeadBody(local.MaxReportDistance / 4f);
    }

    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local) return;

        if (local.TryGetModifier<ExorcistDragModifier>(out var dragMod) && dragMod.DeadBody != null)
        {
            ExorcistRuntime.RpcStopDragging(local, (Vector2)dragMod.DeadBody.transform.position);
            Timer = Cooldown;
            return;
        }

        if (Target == null) return;
        ExorcistRuntime.RpcStartDragging(local, Target.ParentId);
    }

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local) return false;
        var dragging = local.HasModifier<ExorcistDragModifier>();
        return base.CanUse() && (dragging || Target != null) && !local.inVent && (!dragging || CanDrop());
    }

    private bool CanDrop()
    {
        if (Target == null && !PlayerControl.LocalPlayer.TryGetModifier<ExorcistDragModifier>(out _))
            return false;
        // If dragging, can drop at current position (no target required).
        if (PlayerControl.LocalPlayer.HasModifier<ExorcistDragModifier>()) return true;
        return !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider,
            PlayerControl.LocalPlayer.Collider.bounds.center, Target!.TruePosition, Constants.ShipAndAllObjectsMask,
            false);
    }

    public void SetDrag()
    {
        OverrideSprite(TouImpAssets.DragSprite.LoadAsset());
        OverrideName(MiraLocaleManager.Get("TouMarmo.Ability.ExorcistDrag", "Drag"));
    }

    public void SetDrop()
    {
        OverrideSprite(TouImpAssets.DropSprite.LoadAsset());
        OverrideName(MiraLocaleManager.Get("TouMarmo.Ability.ExorcistDrop", "Drop"));
    }

    public override bool IsTargetValid(DeadBody? target) => target && target?.Reported == false;
}
