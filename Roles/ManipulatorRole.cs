using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using UnityEngine;
namespace TouMarmo.Roles;

public sealed class ManipulatorRole(IntPtr ptr) : ImpostorRole(ptr), ITownOfUsRole
{
    public string IdPart => "Manipulator";
    public string RoleNameLocale => "TouMarmo.Role.Manipulator";
    public string RoleDescriptionLocale => "TouMarmo.Role.Manipulator.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Manipulator.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Manipulator.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Manipulator.WikiDescription";
    public Color RoleColor => TownOfUs.TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.ImpSup;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = Assets.ManipulatorAssets.Role,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(Assets.ManipulatorAssets.Role.LoadAsset(), "TouMarmo.Role.Manipulator", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        UseVanillaKillButton = false,
        CanUseVent = true,
        MaxRoleCount = 1,
    };
    public override void Initialize(PlayerControl player) => RoleBehaviourStubs.Initialize(this, player);
    public override void Deinitialize(PlayerControl player)
    {
        Modules.Manipulator.ManipulatorRuntime.CancelFor(player);
        RoleBehaviourStubs.Deinitialize(this, player);
    }
}
