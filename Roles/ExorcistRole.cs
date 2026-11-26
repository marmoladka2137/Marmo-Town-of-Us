using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class ExorcistRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public override bool IsAffectedByComms => false;
    public string IdPart => "Exorcist";
    public string RoleNameLocale => "TouMarmo.Role.Exorcist";
    public string RoleDescriptionLocale => "TouMarmo.Role.Exorcist.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Exorcist.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Exorcist.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Exorcist.WikiDescription";

    // Pale violet / holy tone.
    public Color RoleColor => new Color32(177, 156, 217, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewInvest;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouCrewAssets.CleanseSprite,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouCrewAssets.CleanseSprite.LoadAsset(), "TouMarmo.Role.Exorcist", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        CanUseVent = false,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
    public override void Deinitialize(PlayerControl player)
    {
        ExorcistRuntime.DropIfDragging(player);
        RoleBehaviourStubs.Deinitialize(this, player);
    }
}
