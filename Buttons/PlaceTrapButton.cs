using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class PlaceTrapButton : TownOfUsRoleButton<ComposterRole>
{
    public override string Name => MiraLocaleManager.Get("ComposterAddon.Ability.PlaceTrap", "Place Trap");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => ComposterColors.Role;
    public override float Cooldown => Math.Clamp(
        OptionGroupSingleton<ComposterOptions>.Instance.TrapCooldown.Value + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound.Value;
    public override bool ZeroIsInfinite => true;
    public override bool UsableFirstRound => true;
    public override LoadableAsset<Sprite> Sprite => ComposterAssets.TrapButton;

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        var trapId = Role.GetNextTrapId();

        ComposterRole.RpcPlaceTrap(player, player.GetTruePosition(), trapId);
        TouAudio.PlaySound(TouAudio.TrapperPlaceSound);
    }
}
