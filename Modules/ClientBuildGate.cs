using System.Reflection;
using HarmonyLib;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;
using TownOfUs.Networking;
using UnityEngine;
namespace TouMarmo.Modules;

public static class ClientBuildGate
{
    private static readonly Dictionary<uint,NetData> pendingOptions=new();
    private static int hostId=int.MinValue;
    private static float deadline,nextHello;
    private static bool rejected;
    private static readonly MethodInfo ApplyOptions=AccessTools.Method(typeof(ModdedOptionsManager),"HandleSyncOptions");
    public static PlayerControl? Host => AmongUsClient.Instance ? PlayerControl.AllPlayerControls.ToArray()
        .FirstOrDefault(p=>p && p.OwnerId==AmongUsClient.Instance.HostId) : null;
    public static bool Bypass => !AmongUsClient.Instance || AmongUsClient.Instance.AmHost || TutorialManager.InstanceExists;
    public static bool Trusted => Bypass || Host is { } p && BuildCompatibility.Verified(p);
    public static void Reset()
    {
        hostId=int.MinValue;deadline=nextHello=0;rejected=false;pendingOptions.Clear();
    }
    private static void Hello() => Rpc<SendClientModInfoRpc>.Instance.Send(PlayerControl.LocalPlayer,
        new Dictionary<byte,string>{{BuildCompatibility.ManifestKey,BuildCompatibility.Manifest}});
    public static void Receive(PlayerControl peer)
    {
        var c=AmongUsClient.Instance;
        if(!c || !peer || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if(c.AmHost)
        {
            if(!peer.AmOwner)
            {
                Hello(); // Return the host fingerprint before sending typed settings.
                if(BuildCompatibility.Verified(peer))
                    AccessTools.Method(typeof(ModdedOptionsManager),"SyncAllOptions").Invoke(null,new object[]{peer.OwnerId});
            }
            return;
        }
        if(peer.OwnerId!=c.HostId) return;
        if(hostId!=int.MinValue && hostId!=c.HostId) pendingOptions.Clear();
        hostId=c.HostId;
        if(!BuildCompatibility.Verified(peer)) { Reject();return; }
        if(pendingOptions.Count==0) return;
        var data=pendingOptions.Values.ToArray();pendingOptions.Clear();
        ApplyOptions.Invoke(null,new object[]{data});
    }
    public static void Tick()
    {
        var c=AmongUsClient.Instance;
        if(!c || c.GameState==InnerNet.InnerNetClient.GameStates.NotJoined){Reset();return;}
        if(Bypass || rejected || !LobbyBehaviour.Instance || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if(hostId!=c.HostId){if(hostId!=int.MinValue) pendingOptions.Clear();hostId=c.HostId;deadline=Time.unscaledTime+12;nextHello=0;}
        if(Trusted) return;
        if(Time.unscaledTime>=deadline){Reject();return;}
        if(Time.unscaledTime>=nextHello){nextHello=Time.unscaledTime+2;Hello();}
    }
    public static void Reject()
    {
        if(rejected || Bypass) return;
        rejected=true;pendingOptions.Clear();
        const string reason="MTOU: incompatible or unverified host. All players need the exact same MTOU and MiraAPI build. Original TOU Mira and older MTOU builds are not compatible.";
        Warning(reason);
        AmongUsClient.Instance.LastCustomDisconnect=reason;
        AmongUsClient.Instance.ExitGame(DisconnectReasons.Custom);
        AmongUsClient.Instance.LastCustomDisconnect=reason;
    }
    public static bool Options(NetData[] data)
    {
        if(Trusted) return true;
        // Options arrive in chunks. Merge by ID until the host is authenticated;
        // never interpret another build's numeric option IDs in the local schema.
        if(!rejected && data!=null)
            foreach(var item in data)
                if(pendingOptions.Count<10000 && item.Data!=null && item.Data.Length<=65536)
                    pendingOptions[item.Id]=new NetData(item.Id,item.Data.ToArray());
        return false;
    }
}

[HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
public static class ClientBuildGateUpdate
{
    [HarmonyPostfix] public static void Postfix()=>ClientBuildGate.Tick();
}
[HarmonyPatch(typeof(LobbyBehaviour),nameof(LobbyBehaviour.Start))]
public static class ClientBuildGateLobby
{
    [HarmonyPrefix] public static void Prefix()=>ClientBuildGate.Reset();
}
[HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.CoStartGameClient))]
public static class ClientBuildGateStart
{
    [HarmonyPrefix] public static bool Prefix(ref Il2CppSystem.Collections.IEnumerator __result)
    {
        if(ClientBuildGate.Trusted) return true;
        __result=new Il2CppSystem.Collections.ArrayList().GetEnumerator();
        ClientBuildGate.Reject();return false;
    }
}
[HarmonyPatch]
public static class ClientBuildOptionsGate
{
    public static MethodBase TargetMethod()=>AccessTools.Method(typeof(ModdedOptionsManager),"HandleSyncOptions");
    [HarmonyPrefix] public static bool Prefix(NetData[] data)=>ClientBuildGate.Options(data);
}
[HarmonyPatch]
public static class ClientBuildModifierGate
{
    public static MethodBase TargetMethod()=>AccessTools.Method(typeof(MiraApiPlugin).Assembly.GetType("MiraAPI.Networking.Modifiers.AddModifierRpc"),"Handle");
    [HarmonyPrefix] public static bool Prefix(MiraAPI.Networking.Modifiers.ModifierData data)
    {
        if(!ClientBuildGate.Trusted) return false;
        // This arrow is created locally from the first corpse. It is not a
        // parameterless network modifier. Never turn a mis-mapped RPC into it.
        return ModifierManager.GetModifierType(data.TypeId)!=typeof(TouMarmo.Modifiers.JesterFirstBodyArrowModifier);
    }
}
