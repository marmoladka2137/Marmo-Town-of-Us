using MiraAPI.Patches.Stubs;
using TownOfUs.Interfaces;
using MiraAPI.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
namespace TouMarmo.Roles;
public sealed class DoctorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IVisibleRole
{
    public override bool IsAffectedByComms => false;
    public bool CanOtherRoleSee(RoleBehaviour role, out bool consideredTeammates)
    {
        consideredTeammates = false;
        return Player && DoctorRuntime.For(Player).Revealed;
    }
    public string IdPart => "Doctor";
    public string RoleNameLocale => "TouMarmo.Role.Doctor";
    public string RoleDescriptionLocale => "TouMarmo.Role.Doctor.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Doctor.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Doctor.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Doctor.WikiDescription";
    public Color RoleColor => new Color32(70, 202, 220, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewProc;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouMarmo.Assets.MarmoAssets.Doctor,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Doctor.LoadAsset(), "TouMarmo.Role.Doctor", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 3,
    };
    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
}
