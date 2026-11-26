using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Modules;
using TouMarmo.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class WladcaRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public string IdPart => "Wladca";
    public string RoleNameLocale => "TouMarmo.Role.Wladca";
    public string RoleDescriptionLocale => "TouMarmo.Role.Wladca.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Wladca.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Wladca.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Wladca.WikiDescription";
    public Color RoleColor => new Color32(235, 188, 70, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewSup;
    public float EmpoweredUntil { get; private set; }
    public bool Empowered => Time.time < EmpoweredUntil && !Player.HasDied();
    public bool HasImpostorVision => Empowered && OptionGroupSingleton<WladcaOptions>.Instance.ImpostorVision.Value;
    public override bool IsAffectedByComms => false;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouMarmo.Assets.MarmoAssets.Wladca,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.Wladca.LoadAsset(), "TouMarmo.Role.Wladca", 1.45f),
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        EmpoweredUntil = 0f;
    }

    public override void Deinitialize(PlayerControl player)
    {
        RoleBehaviourStubs.Deinitialize(this, player);
        EmpoweredUntil = 0f;
    }

    [MethodRpc((uint)MarmoRpc.WladcaEmpower, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcEmpower(PlayerControl player)
    {
        if (LobbyBehaviour.Instance || MeetingHud.Instance || player.Data.Role is not WladcaRole role ||
            player.HasDied())
        {
            return;
        }

        role.EmpoweredUntil = Time.time + OptionGroupSingleton<WladcaOptions>.Instance.Duration.Value;
        if (player.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TouMarmo.Role.Wladca.EmpoweredNotice", "Your powers are active."),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouMarmo.Assets.MarmoAssets.Wladca.LoadAsset());
        }
    }
}
