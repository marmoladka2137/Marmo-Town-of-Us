using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TouMarmo.Modifiers;
using TownOfUs.Modifiers.Game.Assailant;
using UnityEngine;
namespace TouMarmo.Modules;
public static class MarmoSession
{
    private static bool inLobby;
    public static PlayerControl? Player(byte id) => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p=>p.PlayerId==id);
    public static bool Alive(PlayerControl? p) => p && p!.Data!=null && !p.Data.Disconnected && !p.HasDied();
    public static bool IsHost => PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsHost();
    public static bool InRound => FurasRuntime.InRound && AmongUsClient.Instance && (TutorialManager.InstanceExists || !AmongUsClient.Instance.IsGameOver);
    public static void Notify(string text) => Helpers.CreateAndShowNotification(text,Color.white,new Vector3(0,1,-20));
    public static void Clear()
    {
        FurasRuntime.Clear();
        DoctorWardrobe.Clear();
        SoulguardRole.ClearChatKnowledge();
        ChanceMisfireRuntime.Clear(); OneMoreChanceModifier.ClearHistory(); ProtectionRuntime.Clear(); DirtekBondRuntime.Clear(); NecromancerRuntime.Clear(); BanditRuntime.Clear(); DoctorRuntime.States.Clear(); ExecutionerChoice.Reset(); SweetsRuntime.Clear(); ExorcistRuntime.Clear();
    }
    [RegisterEvent] public static void End(GameEndEvent e) { FurasRuntime.Clear(); ChanceMisfireRuntime.Clear(); ProtectionRuntime.Clear(); DirtekBondRuntime.Clear(); NecromancerRuntime.Clear(); BanditRuntime.RestoreAll(); SweetsRuntime.Clear(); ExorcistRuntime.Clear(); }
    public static void Tick()
    {
        TouMarmo.Modules.Profile.CosmeticSelectionRuntime.Tick();
        var lobby=LobbyBehaviour.Instance || !ShipStatus.Instance;
        if(lobby){if(!inLobby) Clear(); inLobby=true; return;}
        inLobby=false;
        // Practice dummies can spawn after TOU initializes its death history.
        // Its confirmed kill and revive handlers both index PlayerStats by player ID.
        if (TutorialManager.InstanceExists)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player && player.Data && player.Data.Role && !TownOfUs.Modules.GameHistory.PlayerStats.ContainsKey(player.PlayerId))
                    TownOfUs.Modules.GameHistory.RegisterRole(player, player.Data.Role);
        }
        DirtekBondRuntime.Tick(); BanditRuntime.Tick(); NecromancerRuntime.Tick(); DoctorRuntime.Tick();
        // A shot modifier always retains the guessing ability, including mid-meeting grants.
        if (IsHost)
            foreach (var player in PlayerControl.AllPlayerControls.ToArray())
                if (Alive(player) && player.HasModifier<DoubleShotModifier>() && !player.GetModifierComponent().HasModifier<AssassinModifier>(true))
                    player.RpcAddModifier<AssassinModifier>();
        // Also enforce exclusivity when a different addon directly grants Double Shot.
        foreach(var p in PlayerControl.AllPlayerControls.ToArray())
            if(p.HasModifier<TripleShotModifier>())
                foreach(var mod in p.GetModifiers<DoubleShotModifier>().Where(m=>m.GetType()==typeof(DoubleShotModifier)).ToArray())
                    p.GetModifierComponent().RemoveModifier(mod);
    }
}
[HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
public static class MarmoSessionUpdate
{
    [HarmonyPostfix] public static void Postfix() => MarmoSession.Tick();
}

