using TownOfUs;
using BepInEx.Logging;
using TouMarmo.Modules;
using TownOfUs.Buttons;
using TownOfUs.Roles.Neutral;
using UnityEngine;
namespace TouMarmo.Buttons;
public sealed class ExecutionerNewTargetButton : TownOfUsRoleButton<ExecutionerRole>
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.ExecutionerChoice");
    public override string Name => "New Target";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Executioner;
    public override float Cooldown => 0;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.NewTarget;
    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role) && ExecutionerChoice.Options.ChangeTarget.Value && ExecutionerChoice.ChoiceOpen(PlayerControl.LocalPlayer);
    public override bool CanUse() => base.CanUse() && ExecutionerChoice.CanChoose(PlayerControl.LocalPlayer) && ExecutionerChoice.HasNewTarget(PlayerControl.LocalPlayer);
    protected override void OnClick()
    {
        Log.LogInfo($"Executioner New Target clicked (canUse={CanUse()}, inRound={FurasRuntime.InRound}, isHost={MarmoSession.IsHost}, role={(PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && PlayerControl.LocalPlayer.Data.Role ? PlayerControl.LocalPlayer.Data.Role.GetType().Name : "<none>")}).");
        ExecutionerChoice.RpcRequest(PlayerControl.LocalPlayer, false);
    }
}
public sealed class ExecutionerRenounceButton : TownOfUsRoleButton<ExecutionerRole>
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.ExecutionerChoice");
    public override string Name => "Renounce";
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Executioner;
    public override float Cooldown => 0;
    public override float InitialCooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouMarmo.Assets.MarmoAssets.Renounce;
    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role) && ExecutionerChoice.Options.Renounce.Value && ExecutionerChoice.ChoiceOpen(PlayerControl.LocalPlayer);
    public override bool CanUse() => base.CanUse() && ExecutionerChoice.CanChoose(PlayerControl.LocalPlayer);
    protected override void OnClick()
    {
        Log.LogInfo($"Executioner Renounce clicked (canUse={CanUse()}, inRound={FurasRuntime.InRound}, isHost={MarmoSession.IsHost}, role={(PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && PlayerControl.LocalPlayer.Data.Role ? PlayerControl.LocalPlayer.Data.Role.GetType().Name : "<none>")}).");
        ExecutionerChoice.RpcRequest(PlayerControl.LocalPlayer, true);
    }
}


