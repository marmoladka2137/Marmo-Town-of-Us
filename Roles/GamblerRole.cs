using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Assets;
using TouMarmo.Modules;
using TouMarmo.Options;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class GamblerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole
{
    public string IdPart => "Gambler";
    public string RoleNameLocale => "TouMarmo.Role.Gambler";
    public string RoleDescriptionLocale => "TouMarmo.Role.Gambler.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Gambler.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Gambler.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Gambler.WikiDescription";
    public Color RoleColor => new Color32(218, 155, 39, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.NeutralEvil;
    [HideFromIl2Cpp] public HashSet<byte> Marks { get; } = [];
    [HideFromIl2Cpp] public HashSet<byte> ActiveMarks { get; } = [];
    public bool RoundPending { get; set; }
    public bool ReadyToShoot { get; set; }
    public byte LastShotTargetId { get; private set; } = byte.MaxValue;
    public bool HasWon { get; set; }
    public bool MetWinCon => HasWon;
    public bool WinConditionMet() => false; // Personal win is retained by DidWin; it must not end the match.

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MarmoAssets.Gambler,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(MarmoAssets.Gambler.LoadAsset(), "TouMarmo.Role.Gambler", 1.45f),
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        UseVanillaKillButton = false,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Marks.Clear();
        ActiveMarks.Clear();
        ReadyToShoot = false;
        LastShotTargetId = byte.MaxValue;
        HasWon = false;
    }

    public override void Deinitialize(PlayerControl player)
    {
        base.Deinitialize(player);
        Marks.Clear();
        ActiveMarks.Clear();
        ReadyToShoot = false;
        LastShotTargetId = byte.MaxValue;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        Marks.Clear();
        ActiveMarks.Clear();
        RoundPending = true;
    }

    public override bool DidWin(GameOverReason reason) => HasWon;

    public void StartNextRound()
    {
        if (!RoundPending) return;
        RoundPending = false;
        ActiveMarks.Clear();
        foreach (var id in Marks)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == id);
            if (MarmoSession.Alive(player))
            {
                ActiveMarks.Add(id);
            }
        }
        Marks.Clear();
        ReadyToShoot = false;
        LastShotTargetId = byte.MaxValue;
    }

    [MethodRpc((uint)MarmoRpc.GamblerMark, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMark(PlayerControl gambler, PlayerControl target)
    {
        if (!MeetingHud.Instance || gambler.Data.Role is not GamblerRole role || !MarmoSession.Alive(gambler) ||
            !MarmoSession.Alive(target) || target == gambler || role.Marks.Count >=
            (int)TouMarmo.Modules.AbilityLimits.Count(OptionGroupSingleton<GamblerOptions>.Instance.MaxMarks.Value))
        {
            return;
        }

        role.Marks.Add(target.PlayerId);
        if (gambler.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TouMarmo.Role.Gambler.MarkNotice", "Marked <player> for the next round.")
                    .Replace("<player>", target.Data.PlayerName),
                Color.white, new Vector3(0f, 1f, -20f), spr: MarmoAssets.Mark.LoadAsset());
        }
    }

    [MethodRpc((uint)MarmoRpc.GamblerShoot, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcShoot(PlayerControl gambler, PlayerControl target)
    {
        if (LobbyBehaviour.Instance || gambler.Data.Role is not GamblerRole role ||
            !MarmoSession.Alive(gambler) || !MarmoSession.Alive(target) || target == gambler || !role.ReadyToShoot)
        {
            return;
        }

        if (MeetingHud.Instance && MeetingHud.Instance.CurrentState is not (MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted)) return;
        if (!MeetingHud.Instance && (Vector2.Distance(gambler.GetTruePosition(),target.GetTruePosition()) > gambler.Data.Role.GetAbilityDistance() ||
            !FurasRuntime.ClearPath(gambler.GetTruePosition(), target.GetTruePosition()))) return;
        role.ReadyToShoot = false;
        role.LastShotTargetId = target.PlayerId;
        if (MarmoSession.IsHost)
        {
            if (MeetingHud.Instance)
            {
                gambler.RpcMeetingMurder(target, MeetingAnimation.PlayerNameplateAnimation,
                    CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate), causeOfDeath: "Gambler");
                return;
            }
            gambler.RpcSpecialMurder(target, MeetingCheck.OutsideMeeting,
                resetKillTimer: false, causeOfDeath: "Gambler");
        }
    }
}
