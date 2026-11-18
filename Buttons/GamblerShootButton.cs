using MiraAPI.GameOptions;
using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class GamblerShootButton : TownOfUsRoleButton<GamblerRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.CashOut", "Cash Out");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(218, 155, 39, 255);
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<GamblerOptions>.Instance.ShootCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => MarmoAssets.CashOut;
    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role);
    public override bool CanUse() => base.CanUse() && Role.ReadyToShoot;
    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target != null)
        {
            GamblerRole.RpcShoot(PlayerControl.LocalPlayer, Target);
        }
    }
}
