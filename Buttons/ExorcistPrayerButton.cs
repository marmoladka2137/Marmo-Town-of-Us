using TouMarmo.Modules.Exorcist;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class ExorcistPrayerButton : TownOfUsRoleButton<ExorcistRole>
{
    private float requestedAt = -10f;
    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.ExorcistPrayer", "Prayer");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => new Color32(177, 156, 217, 255);
    public override float Cooldown => ExorcistRuntime.Options.PrayerCooldown.Value;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CleanseSprite;

    public override bool CanUse() => base.CanUse() && ExorcistRuntime.CanUsePrayer(PlayerControl.LocalPlayer);

    public override void ClickHandler()
    {
        if (!CanClick() || Time.unscaledTime - requestedAt < .3f) return;
        requestedAt = Time.unscaledTime;
        OnClick();
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        SetTimer(ExorcistRuntime.PrayerCooldownRemaining(playerControl));
        base.FixedUpdateHandler(playerControl);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (!Button) return;
        var remaining = Mathf.Max(0, (int)TouMarmo.Modules.AbilityLimits.Count(ExorcistRuntime.Options.PrayerUses.Value) - ExorcistRuntime.PrayersUsed);
        Button!.SetUsesRemaining(remaining);
        if (ExorcistRuntime.Options.PrayerUses.Value == 0 && Button.usesRemainingText) Button.usesRemainingText.text="∞";
    }

    protected override void OnClick() => ExorcistRuntime.RpcPrayerRequest(PlayerControl.LocalPlayer);
}
