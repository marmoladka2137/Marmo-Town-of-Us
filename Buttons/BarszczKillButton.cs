using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class BarszczKillButton
    : TownOfUsKillRoleButton<BarszczRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => BarszczColors.Role;
    public override float Cooldown => Math.Clamp(
        OptionGroupSingleton<BarszczOptions>.Instance.KillCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => BarszczAssets.KillButton;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Barszcz kill: target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}
