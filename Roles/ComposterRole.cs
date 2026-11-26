using TouMarmo.Assets;
using TouMarmo.Modules;
using TouMarmo.Options;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Roles;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class ComposterRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Composter";

    public string RoleNameLocale => "ComposterAddon.Role.Composter";
    public string RoleDescriptionLocale => "ComposterAddon.Role.Composter.IntroBlurb";
    public string RoleMedDescriptionLocale => "ComposterAddon.Role.Composter.MedDescription";
    public string RoleLongDescriptionLocale => "ComposterAddon.Role.Composter.TabDescription";
    public string RoleWikiDescriptionLocale => "ComposterAddon.Role.Composter.WikiDescription";

    public int PlacedThisRound { get; private set; }
    public int NextTrapId { get; private set; }

    [HideFromIl2Cpp]
    public HashSet<int> ActiveTrapIds { get; } = [];

    public Color RoleColor => ComposterColors.Role;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.CrewKiller;

    public bool IsPowerCrew => !Player.HasDied() &&
        (ActiveTrapIds.Count > 0 ||
         PlacedThisRound < (int)TouMarmo.Modules.AbilityLimits.Count(TouMarmo.Modules.AbilityLimits.Count(OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound.Value)));

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            ComposterAssets.RoleIcon.LoadAsset(), "ComposterAddon.Role.Composter", 1.45f),
        Icon = ComposterAssets.RoleIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.SneakyIntro,
    };

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get(RoleWikiDescriptionLocale) + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get("ComposterAddon.Ability.PlaceTrap", "Place Trap"),
            MiraLocaleManager.Get("ComposterAddon.Ability.PlaceTrap.WikiDescription"),
            ComposterAssets.TrapButton),
    ];

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get("ComposterAddon.Ability.PlaceTrap", "Place Trap"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Indirect"),
            MiraLocaleManager.Get("ComposterAddon.Ability.PlaceTrap.WikiDescription"),
            ComposterAssets.TrapButton),
    ];

    public int GetNextTrapId() => NextTrapId++;

    public void LobbyStart() => ResetRound();

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ResetRound();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ActiveTrapIds.Clear();
        CompostTrap.ClearFor(targetPlayer.PlayerId);
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        ClearTraps();
    }

    public void ResetRound()
    {
        ClearTraps();
        PlacedThisRound = 0;
        NextTrapId = 0;
    }

    public void ClearTraps()
    {
        ActiveTrapIds.Clear();
        if (Player != null)
        {
            CompostTrap.ClearFor(Player.PlayerId);
        }
    }

    [MethodRpc((uint)ComposterRpc.PlaceTrap, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPlaceTrap(PlayerControl player, Vector2 position, int trapId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player.Data.Role is not ComposterRole role || player.HasDied() || player.Data.Disconnected)
        {
            Error("RpcPlaceTrap - invalid Composter");
            return;
        }

        var maximum = (int)TouMarmo.Modules.AbilityLimits.Count(TouMarmo.Modules.AbilityLimits.Count(OptionGroupSingleton<ComposterOptions>.Instance.MaxTrapsPerRound.Value));
        if (role.PlacedThisRound >= maximum || role.ActiveTrapIds.Contains(trapId) ||
            Vector2.Distance(player.GetTruePosition(), position) > 1.5f)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        role.PlacedThisRound++;
        role.ActiveTrapIds.Add(trapId);

        if (player.AmOwner)
        {
            CompostTrap.Create(player, trapId, position);
        }
    }

    [MethodRpc((uint)ComposterRpc.TriggerTrap, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcTriggerTrap(PlayerControl composter, PlayerControl target, int trapId)
    {
        if (LobbyBehaviour.Instance || composter.Data.Role is not ComposterRole role ||
            target == composter || target.HasDied() || !role.ActiveTrapIds.Remove(trapId))
        {
            return;
        }

        CompostTrap.Remove(composter.PlayerId, trapId);

        if (composter.AmOwner)
        {
            composter.RpcSpecialMurder(target, MeetingCheck.OutsideMeeting, isIndirect: true,
                resetKillTimer: false, teleportMurderer: false, showKillAnim: true,
                causeOfDeath: "ComposterAddonTrap");
        }
    }
}
