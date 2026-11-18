using TouMarmo.Modules.Exorcist;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class ExorcistPrayerButton : TownOfUsRoleButton<ExorcistRole>
{
    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.ExorcistPrayer", "Prayer");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => new Color32(177, 156, 217, 255);
    public override float Cooldown => ExorcistRuntime.Options.PrayerCooldown.Value;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CleanseSprite;

    public override bool CanUse()
    {
        if (!base.CanUse()) return false;
        if (!ExorcistRuntime.CanUsePrayer(PlayerControl.LocalPlayer)) return false;
        // Prayer is only worth pressing with enough bodies nearby (and uses left).
        // Do not consume on click if not enough bodies - checked in request/handler, but
        // also keep button usable to allow attempt feedback-free retry? Spec: "Jeżeli warunki
        // nie są spełnione, Prayer nie zostaje zużyte..." so we must not consume; allow click.
        return ExorcistRuntime.PrayersUsed < (int)ExorcistRuntime.Options.PrayerUses.Value;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (!Button) return;
        var remaining = Mathf.Max(0, (int)ExorcistRuntime.Options.PrayerUses.Value - ExorcistRuntime.PrayersUsed);
        Button!.SetUsesRemaining(remaining);
    }

    protected override void OnClick() => ExorcistRuntime.RpcPrayerRequest(PlayerControl.LocalPlayer);
}
