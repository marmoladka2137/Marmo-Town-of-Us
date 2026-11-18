using MiraAPI.GameOptions;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace TouMarmo.Buttons;

public sealed class WladcaEmpowerButton : TownOfUsRoleButton<WladcaRole>
{
    public override string Name => MiraLocaleManager.Get("TouMarmo.Ability.Empower", "Empower");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => new Color32(235, 188, 70, 255);
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<WladcaOptions>.Instance.Cooldown.Value + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<WladcaOptions>.Instance.Uses.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.Empower;

    protected override void OnClick() => WladcaRole.RpcEmpower(PlayerControl.LocalPlayer);
}
