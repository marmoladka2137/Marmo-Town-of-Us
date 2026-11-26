using System.Text.Json;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
namespace TouMarmo.Modules.LobbyMimic;

public static class MimicRuntime
{
    public static readonly MimicSession Session = new();
    private static readonly Dictionary<uint,float> seen = new();
    private static readonly HashSet<uint> announced = new();
    private static readonly HashSet<uint> pending = new();
    private static float nextBroadcast;
    public static bool Active => Session.Active;
    public static bool Locked(PlayerControl? p) => Active && p && Session.Originals.ContainsKey(p!.NetId);
    public static OutfitSnapshot Read(NetworkedPlayerInfo.PlayerOutfit o) => new(o.PlayerName,o.ColorId,o.HatId,o.SkinId,o.VisorId,o.PetId,o.NamePlateId);
    public static void Write(NetworkedPlayerInfo.PlayerOutfit o, OutfitSnapshot s)
    {
        o.PlayerName=s.Name; o.ColorId=s.Color; o.HatId=s.Hat; o.SkinId=s.Skin; o.VisorId=s.Visor; o.PetId=s.Pet; o.NamePlateId=s.NamePlate;
    }
    public static bool CanCommand => AmongUsClient.Instance && MimicSession.CanCommand(AmongUsClient.Instance.AmHost,
        LobbyBehaviour.Instance, AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
    private static bool Host(PlayerControl p) => p && AmongUsClient.Instance && p.OwnerId==AmongUsClient.Instance.HostId;
    private static bool Valid(OutfitSnapshot? s) => s != null && s.Color>=0 && s.Color<Palette.PlayerColors.Length &&
        new[]{s.Name,s.Hat,s.Skin,s.Visor,s.Pet,s.NamePlate}.All(v=>v!=null && v.Length<=256);
    public static void Begin(PlayerControl source)
    {
        if (!CanCommand || !source || !source.Data) return;
        RpcState(PlayerControl.LocalPlayer, Math.Max(Session.Version,Session.Retired)+1, JsonSerializer.Serialize(Read(source.Data.DefaultOutfit)));
    }
    [MethodRpc(4300, LocalHandling=RpcLocalHandling.Before)]
    public static void RpcState(PlayerControl host,int version,string json)
    {
        if (!Host(host) || json.Length>4096) return;
        OutfitSnapshot? target;
        try { target=JsonSerializer.Deserialize<OutfitSnapshot>(json); } catch(JsonException) { return; }
        var changed = !Active || version != Session.Version;
        if (!Valid(target) || !Session.Begin(version,target!)) return;
        if (!changed) return;
        foreach(var p in PlayerControl.AllPlayerControls.ToArray())
            if (p && p.Data && !p.Data.Disconnected && !string.IsNullOrEmpty(p.Data.PlayerName))
            {
                Capture(p);
                pending.Add(p.NetId);
            }
    }
    [MethodRpc(4301, LocalHandling=RpcLocalHandling.Before)]
    public static void RpcStop(PlayerControl host,int version)
    {
        if (Host(host) && version>=Session.Version) Stop(version);
    }
    private static void Capture(PlayerControl p)
    {
        if (!Session.Capture(p.NetId,Read(p.Data.DefaultOutfit))) return;
        pending.Add(p.NetId);
    }
    public static void Apply(PlayerControl p)
    {
        if (!Locked(p) || !p.Data || !p.cosmetics || !ModifierExtensions.ModifierComponents.ContainsKey(p)) return;
        // Existing appearance pipeline preserves scale/speed, invisibility and body type.
        p.RawSetAppearance(p.GetAppearance());
        pending.Remove(p.NetId);
    }
    public static void Stop(int version)
    {
        Session.Stop(version); // Disable overrides before any ResetAppearance call.
        foreach(var p in PlayerControl.AllPlayerControls.ToArray())
            if (p && p.Data && Session.Originals.TryGetValue(p.NetId,out var original))
            {
                try
                {
                    Write(p.Data.DefaultOutfit,original);
                    if(p.cosmetics) p.ResetAppearance(true);
                }
                catch(Exception error) { UnityEngine.Debug.LogWarning($"MTOU Mimic restore failed for net ID {p.NetId}: {error.Message}"); }
            }
        Session.Originals.Clear(); seen.Clear(); announced.Clear(); pending.Clear();
    }
    public static void Exit()
    {
        if(Active) Stop(Session.Version);
        Session.ResetConnection();
    }
    public static void Tick()
    {
        if (!Active) return;
        var client=AmongUsClient.Instance;
        if (!client || client.GameState==InnerNet.InnerNetClient.GameStates.NotJoined) { Exit(); return; }
        if (Session.Started && LobbyBehaviour.Instance) { Stop(Session.Version); return; }
        var players=PlayerControl.AllPlayerControls.ToArray().Where(p=>p && p.Data && !p.Data.Disconnected).ToArray();
        foreach(var p in players)
        {
            if(pending.Contains(p.NetId)) Apply(p);
            if (!seen.TryGetValue(p.NetId,out var at)) {seen[p.NetId]=Time.unscaledTime;continue;}
            if (Time.unscaledTime-at<1f || string.IsNullOrEmpty(p.Data.PlayerName)) continue;
            Capture(p);
            if(pending.Contains(p.NetId)) Apply(p);
            if (client.AmHost && announced.Add(p.NetId)) nextBroadcast=0;
        }
        if(client.AmHost && Time.unscaledTime>=nextBroadcast && Session.Target!=null)
        {
            nextBroadcast=Time.unscaledTime+2;
            // Reliable replay lets a newly joined client receive the active session.
            RpcState(PlayerControl.LocalPlayer,Session.Version,JsonSerializer.Serialize(Session.Target));
        }
    }
    [RegisterEvent]
    public static void Round(RoundStartEvent e) { if(Active && e.TriggeredByIntro) Session.Started=true; }
    [RegisterEvent(10000)]
    public static void End(GameEndEvent e) { if(Active) Stop(Session.Version); }
}

[HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.OnGameEnd))]
public static class MimicGameEnd
{
    [HarmonyPrefix,HarmonyPriority(Priority.First)] public static void Prefix() {if(MimicRuntime.Active) MimicRuntime.Stop(MimicRuntime.Session.Version);}
}
[HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.ExitGame))]
public static class MimicExit
{
    [HarmonyPrefix] public static void Prefix() => MimicRuntime.Exit();
}
