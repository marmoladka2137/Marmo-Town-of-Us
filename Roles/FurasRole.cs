using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TouMarmo.Assets;
using TouMarmo.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TouMarmo.Roles;
public sealed class FurasRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public override bool IsAffectedByComms => false;
    public string IdPart => "Furas";
    public string RoleNameLocale => "TouMarmo.Role.Furas";
    public string RoleDescriptionLocale => "TouMarmo.Role.Furas.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Furas.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Furas.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Furas.WikiDescription";
    public Color RoleColor => new Color32(239, 153, 69, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewKiller;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = FurasAssets.Pet,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(FurasAssets.Pet.LoadAsset(), "TouMarmo.Role.Furas", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 3,
    };
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        FurasRuntime.Reset(player.PlayerId);
    }
    public override void Deinitialize(PlayerControl player)
    {
        FurasRuntime.Reset(player.PlayerId);
        RoleBehaviourStubs.Deinitialize(this, player);
    }
}
