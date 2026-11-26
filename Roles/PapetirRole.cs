using AmongUs.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TouMarmo.Modules.Papetir;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;
namespace TouMarmo.Roles;
public sealed class PapetirRole(IntPtr ptr) : NeutralRole(ptr), ITownOfUsRole
{
    public string IdPart=>"Papetir";
    public string RoleNameLocale=>"TouMarmo.Role.Papetir";
    public string RoleDescriptionLocale=>"TouMarmo.Role.Papetir.IntroBlurb";
    public string RoleMedDescriptionLocale=>"TouMarmo.Role.Papetir.MedDescription";
    public string RoleLongDescriptionLocale=>"TouMarmo.Role.Papetir.TabDescription";
    public string RoleWikiDescriptionLocale=>"TouMarmo.Role.Papetir.WikiDescription";
    public static Color Color=>new Color32(214,199,160,255);
    public Color RoleColor=>Color;
    public ModdedRoleTeams Team=>ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment=>RoleAlignment.NeutralEvil;
    public RoleOptionsGroup RoleOptionsGroup=>TouRoleGroups.NeutralEvil;
    public CustomRoleConfiguration Configuration=>new(this){Icon=PapetirAssets.Paper,
        IconTmp=TmpSpriteUtils.CreateSpriteAsset(PapetirAssets.Paper.LoadAsset(),"TouMarmo.Role.Papetir",1.45f),
        GhostRole=(RoleTypes)RoleId.Get<NeutralGhostRole>(),UseVanillaKillButton=false,CanUseVent=false,MaxRoleCount=1};
    public override void Initialize(PlayerControl p)=>RoleBehaviourStubs.Initialize(this,p);
    public bool WinConditionMet()=>Player && Player.Data && !Player.Data.Disconnected &&
        PapetirRuntime.Ledger.Victims(Player.PlayerId)>=(int)PapetirRuntime.Options.VictimsRequired.Value;
    public bool MetWinCon=>WinConditionMet();
    public override bool DidWin(GameOverReason reason)=>WinConditionMet();
}
