using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
namespace TouMarmo.Roles;
public sealed class BanditRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public string IdPart => "Bandit";
    public string RoleNameLocale => "TouMarmo.Role.Bandit";
    public string RoleDescriptionLocale => "TouMarmo.Role.Bandit.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Bandit.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Bandit.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Bandit.WikiDescription";
    public Color RoleColor => new Color32(135, 105, 75, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewPower;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouMarmo.Assets.MarmoAssets.Bandit,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Bandit.LoadAsset(), "TouMarmo.Role.Bandit", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 3,
    };
    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
}
