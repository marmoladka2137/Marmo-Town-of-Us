using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using UnityEngine;
namespace TouMarmo.Roles;
public sealed class SweetsRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole
{
    public string IdPart => "Sweets";
    public string RoleNameLocale => "TouMarmo.Role.Sweets";
    public string RoleDescriptionLocale => "TouMarmo.Role.Sweets.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Sweets.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Sweets.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Sweets.WikiDescription";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.ImpPower;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouMarmo.Assets.MarmoAssets.Sweets,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Sweets.LoadAsset(), "TouMarmo.Role.Sweets", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        // Sweets keeps the normal Impostor kill; the ability button is Flower/Pollinate.
        UseVanillaKillButton = true,
        CanUseVent = true,
        MaxRoleCount = 1,
    };
    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
}
