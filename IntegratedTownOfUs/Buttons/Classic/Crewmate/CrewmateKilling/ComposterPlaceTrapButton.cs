using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class ComposterPlaceTrapButton : TownOfUsRoleButton<ComposterRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ComposterPlaceTrap", "Place Trap");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Composter;
    public override float Cooldown => Math.Clamp(
        OptionGroupSingleton<ComposterOptions>.Instance.TrapCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound;
    public override bool ZeroIsInfinite => false;
    public override bool UsableFirstRound => true;
    public override LoadableAsset<Sprite> Sprite => ComposterAssets.TrapButton;

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        var trapId = Role.GetNextTrapId();

        ComposterRole.RpcPlaceCompostTrap(player, player.GetTruePosition(), trapId);
        TouAudio.PlaySound(TouAudio.TrapperPlaceSound);
    }
}
