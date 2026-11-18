using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
namespace TouMarmo.Roles;
public sealed class NecromancerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public string IdPart => "Necromancer";
    public string RoleNameLocale => "TouMarmo.Role.Necromancer";
    public string RoleDescriptionLocale => "TouMarmo.Role.Necromancer.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Necromancer.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Necromancer.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Necromancer.WikiDescription";
    public Color RoleColor => new Color32(119, 76, 157, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewPower;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouMarmo.Assets.MarmoAssets.Necromancer,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Necromancer.LoadAsset(), "TouMarmo.Role.Necromancer", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 3,
    };
    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
}
