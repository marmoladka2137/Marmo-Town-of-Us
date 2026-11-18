using MiraAPI.MeetingAbilities;
using TouMarmo.Modules;
using TouMarmo.Roles;
using UnityEngine;
namespace TouMarmo.MeetingAbilities;
public sealed class DoctorRevealButton : MeetingActionButton
{
    public override string Name => LegacyAssets.IsLegacy ? string.Empty : MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianReveal");
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyAssets.RevealButtonSprite : TouAssets.RevealButtonSprite;
    public override bool HideUponWrapUp => true;
    public override bool DisableUponUse => true;
    public override Color TextOutlineColor => new Color32(70,202,220,255);
    public override bool Enabled(RoleBehaviour? role) => role is DoctorRole && PlayerControl.LocalPlayer && !DoctorRuntime.For(PlayerControl.LocalPlayer).Revealed;
    public override bool CanUse() => base.CanUse() && DoctorRuntime.MayAttempt(PlayerControl.LocalPlayer) &&
        MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    protected override void OnClick() => DoctorRuntime.RpcRevealRequest(PlayerControl.LocalPlayer);
}
