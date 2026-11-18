using MiraAPI.GameOptions;
using MiraAPI.MeetingAbilities;
using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using UnityEngine;

namespace TouMarmo.MeetingAbilities;

public sealed class GamblerCashOutButton : TargetedMeetingButton
{
    private readonly List<MeetingAbilityBehaviour> _buttons = [];
    private MeetingHud? _meeting;

    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.CashOut", "Cash Out");
    public override int MaxUses => 1;
    public override MeetingButtonUsesMode UsesMode { get; } = MeetingButtonUsesMode.PerMeeting;
    public override float Cooldown => 0.0001f;
    public override float InitialCooldown => 0.0001f;
    public override LoadableAsset<Sprite> Sprite => MarmoAssets.CashOut;
    public override Color OutlineColor => new Color32(218, 155, 39, 255);
    public override bool Enabled(RoleBehaviour role) => role is GamblerRole gambler && !gambler.Player.HasDied() && gambler.ReadyToShoot;

    public override bool IsTargetValid(PlayerVoteArea area)
    {
        if (!base.IsTargetValid(area) || PlayerControl.LocalPlayer.Data.Role is not GamblerRole role)
        {
            return false;
        }

        var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == area.PlayerId.Value);
        return target != null && !target.HasDied();
    }

    public override MeetingAbilityBehaviour CreateButton(PlayerVoteArea area)
    {
        if (_meeting != MeetingHud.Instance)
        {
            _meeting = MeetingHud.Instance;
            _buttons.Clear();
        }
        var button = base.CreateButton(area);
        _buttons.Add(button);
        return button;
    }

    public override void FixedUpdate() => RefreshButtons();

    public override void ClickHandler(MeetingAbilityBehaviour button, PlayerVoteArea area)
    {
        if (!Enabled(PlayerControl.LocalPlayer.Data.Role) || !IsTargetValid(area))
        {
            return;
        }
        base.ClickHandler(button, area);
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        foreach (var button in _buttons)
        {
            if (!button || !button.VoteArea)
            {
                continue;
            }
            var active = UsesLeft > 0 && Enabled(PlayerControl.LocalPlayer.Data.Role) && IsTargetValid(button.VoteArea);
            if (button.gameObject.activeSelf != active)
            {
                button.gameObject.SetActive(active);
            }
        }
    }

    protected override void OnClick(PlayerVoteArea area)
    {
        var target = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == area.PlayerId.Value);
        if (target != null)
        {
            GamblerRole.RpcShoot(PlayerControl.LocalPlayer, target);
        }
    }
}
