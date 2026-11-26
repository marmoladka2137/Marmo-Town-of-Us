using AmongUs.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TouMarmo.Modules.Gravedigger;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;
namespace TouMarmo.Roles;

public sealed class GravediggerRole(IntPtr ptr) : NeutralRole(ptr), ITownOfUsRole, TownOfUs.Interfaces.IProgressTally
{
    public string IdPart => "Gravedigger";
    public string RoleNameLocale => "TouMarmo.Role.Gravedigger";
    public string RoleDescriptionLocale => "TouMarmo.Role.Gravedigger.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Gravedigger.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Gravedigger.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Gravedigger.WikiDescription";
    public static Color Color => new Color32(139, 119, 91, 255);
    public Color RoleColor => Color;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.NeutralBenign;
    public bool HasBurialWin { get; set; }
    public bool ProgressOnName(bool localDead,bool inMeeting,bool amOwner,out string progress)
    {
        progress=(amOwner || localDead) && Player ? $"({GravediggerRuntime.For(Player).Ledger.Count}/{GravediggerRuntime.Required})" : string.Empty;
        return progress.Length>0;
    }
    public string ProgressOnSummaryNormal => string.Empty;
    public string ProgressOnSummaryDetailed => string.Empty;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = GravediggerAssets.Role,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(GravediggerAssets.Role.LoadAsset(), "TouMarmo.Role.Gravedigger", 1.45f),
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        UseVanillaKillButton = false, CanUseVent = false, MaxRoleCount = 1
    };
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        HasBurialWin = false;
    }
    public override void Deinitialize(PlayerControl player)
    {
        GravediggerRuntime.CloseViews(player);
        base.Deinitialize(player);
    }
    public bool WinConditionMet() => Player && Player.Data && !Player.Data.Disconnected &&
        !Player.HasDied() && (HasBurialWin || GravediggerRuntime.For(Player).Ledger.Won(true, GravediggerRuntime.Required));
    public bool MetWinCon => WinConditionMet();
    public override bool DidWin(GameOverReason reason) => WinConditionMet();
}
