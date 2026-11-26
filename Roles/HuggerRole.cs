using System.Collections;
using TouMarmo.Assets;
using TouMarmo.Modules;
using TouMarmo.Options;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class HuggerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, TownOfUs.Interfaces.IMeetingKiller
{
    public string IdPart => "Hugger";
    public string RoleNameLocale => "ComposterAddon.Role.Hugger";
    public string RoleDescriptionLocale => "ComposterAddon.Role.Hugger.IntroBlurb";
    public string RoleMedDescriptionLocale => "ComposterAddon.Role.Hugger.MedDescription";
    public string RoleLongDescriptionLocale => "ComposterAddon.Role.Hugger.TabDescription";
    public string RoleWikiDescriptionLocale => "ComposterAddon.Role.Hugger.WikiDescription";
    public Color RoleColor => TownOfUs.TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.ImpPower;
    public bool HuggedImpostor { get; private set; }
    public bool HuggedOther { get; private set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = HuggerAssets.Hug,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(HuggerAssets.Hug.LoadAsset(), "ComposterAddon.Role.Hugger", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        UseVanillaKillButton = false,
        CanUseVent = OptionGroupSingleton<HuggerOptions>.Instance.CanVent.Value,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        HuggedImpostor = false;
        HuggedOther = false;
    }

    public override void Deinitialize(PlayerControl player)
    {
        RoleBehaviourStubs.Deinitialize(this, player);
        HuggedImpostor = false;
        HuggedOther = false;
    }

    [MethodRpc((uint)ComposterRpc.HuggerMapHug, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMapHug(PlayerControl hugger, PlayerControl target)
    {
        if (LobbyBehaviour.Instance || MeetingHud.Instance || hugger.Data.Role is not HuggerRole ||
            hugger.HasDied() || target.HasDied() || target == hugger ||
            Vector2.Distance(hugger.GetTruePosition(), target.GetTruePosition()) > 2.5f)
        {
            return;
        }

        if (hugger.AmOwner)
        {
            var createBody = System.Random.Shared.Next(100) >=
                (int)OptionGroupSingleton<HuggerOptions>.Instance.BodyDisappearChance.Value;
            hugger.RpcSpecialMurder(target, MeetingCheck.OutsideMeeting,
                isIndirect: false, resetKillTimer: false, createDeadBody: createBody,
                teleportMurderer: true, showKillAnim: true, causeOfDeath: "Hugger");
        }
    }

    [MethodRpc((uint)ComposterRpc.HuggerMeetingHug, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMeetingHug(PlayerControl hugger, PlayerControl target)
    {
        if (!MeetingHud.Instance || hugger.Data.Role is not HuggerRole role || hugger.HasDied() ||
            target.HasDied() || target == hugger)
        {
            return;
        }

        var isImpostor = target.IsImpostorAligned();
        if (isImpostor ? role.HuggedImpostor : role.HuggedOther)
        {
            return;
        }

        if (isImpostor)
        {
            role.HuggedImpostor = true;
            var needsAssassinUi = target.AmOwner && !target.HasModifier<AssassinModifier>();
            if (hugger.AmOwner)
            {
                if (!target.HasModifier<AssassinModifier>())
                {
                    target.RpcAddModifier<AssassinModifier>();
                }
                if (!target.HasModifier<DoubleShotModifier>())
                {
                    target.RpcAddModifier<DoubleShotModifier>();
                }
            }
            if (target.AmOwner)
            {
                Coroutines.Start(CoActivateMeetingAssassin(target));
            }
        }
        else
        {
            role.HuggedOther = true;
            if (hugger.AmOwner)
            {
                // TOU's meeting murder RPC updates the vote area on every client,
                // including the Hugger, as soon as the host confirms the kill.
                hugger.RpcMeetingMurder(target, MeetingAnimation.RoleSpecific,
                    CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                    causeOfDeath: "Hugger");
            }
        }

        if (target.AmOwner)
        {
            var message = MiraLocaleManager.Get("ComposterAddon.Role.Hugger.MeetingNotice", "Hugger hugged you!");
            Helpers.CreateAndShowNotification(message, Color.white,
                new Vector3(0f, 1f, -20f), spr: HuggerAssets.Hug.LoadAsset());
        }
    }

    public void TriggerMeetingAnimation(PlayerControl source, PlayerControl target, PlayerVoteArea targetVoteArea, int associatedAnimKey = -1)
    {
        Coroutines.Start(CustomTouMurderRpcs.CoAnimateDeath(targetVoteArea, associatedAnimKey, instant: true));
    }

    private static IEnumerator CoActivateMeetingAssassin(PlayerControl target)
    {
        var timeout = 5f;
        while (target && MeetingHud.Instance && timeout > 0f)
        {
            if (target.TryGetModifier<AssassinModifier>(out var modifier) && modifier.Initialized)
            {
                modifier.OnMeetingStart();
                yield break;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }
    }
}
