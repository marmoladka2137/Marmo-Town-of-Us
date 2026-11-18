using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class HuggerMapButton : TownOfUsRoleButton<HuggerRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("ComposterAddon.Ability.Hug", "Hug");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(205, 44, 72, 255);
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HuggerOptions>.Instance.HugCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => HuggerAssets.Hug;

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target != null)
        {
            HuggerRole.RpcMapHug(PlayerControl.LocalPlayer, Target);
        }
    }
}
