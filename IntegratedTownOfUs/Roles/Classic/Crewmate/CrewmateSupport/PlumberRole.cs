// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using System.Collections;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Plumber;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class PlumberRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    [HideFromIl2Cpp] public static HashSet<int> VentFlushSet { get; set; } = [];
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Plumber";
    public Color RoleColor => TownOfUsColors.Plumber;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Plumber.LoadAsset(), "TouMira.Role.Crewmate.Plumber", 1.45f),
        GetsVentData = true, IntroSound = TouAudio.EngineerIntroSound,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner, Icon = TouRoleIcons.Plumber
    };
    public string GetAdvancedDescription() => MiraLocaleManager.Get("TownOfUsMira.Role.Plumber.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Flush", MiraLocaleManager.Get("TownOfUsMira.Role.PlumberFlush.WikiDescription"), TouCrewAssets.FlushSprite),
        new("Nail", MiraLocaleManager.Get("TownOfUsMira.Role.PlumberNail.WikiDescription"), TouMarmo.Assets.MarmoAssets.PlumberNail)
    ];
    [HideFromIl2Cpp] public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new("Flush", MiraLocaleManager.Get("MiraApi.AbilityType.Basic"), MiraLocaleManager.Get("TownOfUsMira.Role.PlumberFlush.WikiDescription"), TouCrewAssets.FlushSprite),
        new("Nail", MiraLocaleManager.Get("MiraApi.AbilityType.Basic"), MiraLocaleManager.Get("TownOfUsMira.Role.PlumberNail.WikiDescription"), TouMarmo.Assets.MarmoAssets.PlumberNail)
    ];
    [HideFromIl2Cpp] public StringBuilder SetTabText() => ITownOfUsRole.SetNewTabText(this);
    public void LobbyStart() => ClearAll();
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (TutorialManager.InstanceExists) ClearAll();
    }
    public override void Deinitialize(PlayerControl targetPlayer) => RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    public void Clear() => ClearAll();
    public static void ClearAll() { VentFlushSet.Clear(); PlumberNailRuntime.Clear(); }
    public static IEnumerator SeeVenter(PlayerControl plumber)
    {
        var playersInVent = PlayerControl.AllPlayerControls.ToArray().Where(x => x.inVent);

        foreach (var player in playersInVent)
        {
            player.AddModifier<PlumberVenterModifier>(plumber, Color.white);
        }

        yield return new WaitForSeconds(1f);

        foreach (var player in ModifierUtils.GetPlayersWithModifier<PlumberVenterModifier>(x => x.Owner == plumber))
        {
            player.RemoveModifier<PlumberVenterModifier>();
        }
    }

    public static IEnumerator SetupFlush(int id)
    {
        var delay = OptionGroupSingleton<PlumberOptions>.Instance.FlushDuration;
        VentFlushSet.Add(id);

        yield return new WaitForSeconds(delay);

        VentFlushSet.Remove(id);
    }

    [MethodRpc((uint)TownOfUsRpc.PlumberFlush)]
    public static void RpcPlumberFlush(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not PlumberRole)
        {
            Error("RpcPlumberFlush - Invalid Plumber");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.PlumberFlush, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var someoneInVent = PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent);
        if (!someoneInVent)
        {
            return;
        }

        if (player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
            Coroutines.Start(SeeVenter(player));
        }

        if (PlayerControl.LocalPlayer.inVent)
        {
            RpcPlumberSendFlush(PlayerControl.LocalPlayer, Vent.currentVent.Id);
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);

            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PlumberSendFlush)]
    public static void RpcPlumberSendFlush(PlayerControl player, int ventId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        Coroutines.Start(SetupFlush(ventId));
    }

}

