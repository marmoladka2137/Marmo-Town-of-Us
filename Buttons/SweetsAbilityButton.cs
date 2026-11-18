using TouMarmo.Modules.Sweets;
using TouMarmo.Roles;
using TownOfUs;
using TownOfUs.Buttons;
using UnityEngine;
namespace TouMarmo.Buttons;

/// <summary>
/// Sweets' single ability button. It hands out flowers to non-Impostors and,
/// once the configured number of flowers has been given, morphs into Pollinate.
/// </summary>
public sealed class SweetsAbilityButton : TownOfUsRoleButton<SweetsRole, PlayerControl>
{
    private bool _showingPollinate;

    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.Flower", "Flower");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => SweetsRuntime.Options.FlowerCooldown.Value;
    public override float InitialCooldown => 0;
    public override float Distance => SweetsRuntime.Distance;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.SweetsFlower;

    // The same button remains available for every Flower/Pollinate cycle.
    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role);

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        // Force the next FixedUpdate to re-apply the correct Flower/Pollinate look.
        _showingPollinate = false;
    }

    public override PlayerControl? GetTarget()
    {
        // Pollinate has no target; the local player keeps the base target check satisfied.
        if (SweetsRuntime.PollinateMode) return PlayerControl.LocalPlayer;
        // includeImpostors: false - a flower can only be given to a non-Impostor player.
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, false,
            p => !SweetsRuntime.Flowers.Contains(p.PlayerId) && !p.IsImpostorAligned());
    }

    public override void SetOutline(bool active)
    {
        if (SweetsRuntime.PollinateMode) return;
        base.SetOutline(active);
    }

    public override bool CanUse()
    {
        if (SweetsRuntime.PollinateMode)
            return base.CanUse() && SweetsRuntime.CanPollinate(PlayerControl.LocalPlayer);
        return base.CanUse() && Target != null && SweetsRuntime.CanGive(PlayerControl.LocalPlayer, Target);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        var pollinate = SweetsRuntime.PollinateMode;
        if (pollinate != _showingPollinate)
        {
            _showingPollinate = pollinate;
            OverrideName(MiraLocaleManager.Get(
                pollinate ? "TouMarmo.Ability.Pollinate" : "TouMarmo.Ability.Flower",
                pollinate ? "Pollinate" : "Flower"));
            OverrideSprite((pollinate ? TouMarmo.Assets.MarmoAssets.SweetsPollinate : TouMarmo.Assets.MarmoAssets.SweetsFlower).LoadAsset());
        }

        if (Button == null) return;

        // Show progress of distributed flowers as given/required (e.g. 1/3) while giving flowers.
        if (!pollinate)
        {
            var given = Mathf.Clamp(SweetsRuntime.FlowersGiven, 0, SweetsRuntime.RequiredFlowers);
            var required = SweetsRuntime.RequiredFlowers;
            Button.SetUsesRemaining(required);
            if (Button.usesRemainingText != null)
            {
                Button.usesRemainingText.text = $"{given}/{required}";
                Button.usesRemainingText.gameObject.SetActive(true);
            }
            if (Button.usesRemainingSprite != null)
                Button.usesRemainingSprite.gameObject.SetActive(true);
        }
        else
        {
            if (Button.usesRemainingText != null)
                Button.usesRemainingText.gameObject.SetActive(false);
            if (Button.usesRemainingSprite != null)
                Button.usesRemainingSprite.gameObject.SetActive(false);
        }
    }

    protected override void OnClick()
    {
        if (SweetsRuntime.PollinateMode)
        {
            SweetsRuntime.RpcPollinateRequest(PlayerControl.LocalPlayer);
            return;
        }
        if (Target) SweetsRuntime.RpcFlowerRequest(PlayerControl.LocalPlayer, Target!);
    }
}
