param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$code=@'
#nullable enable annotations
using System;using System.Linq;using System.Reflection;using System.Collections.Generic;using HarmonyLib;using static FakeLog;
using MiraAPI;using MiraAPI.GameOptions;using MiraAPI.Modifiers;using MiraAPI.Networking;using Reactor.Networking.Rpc;using TownOfUs.Networking;
namespace HarmonyLib {public class HarmonyPatch:Attribute{public HarmonyPatch(){}public HarmonyPatch(Type t,string n){}}public class HarmonyPrefix:Attribute{}public class HarmonyPostfix:Attribute{}public class HarmonyPriority:Attribute{public HarmonyPriority(int x){}}public static class Priority{public const int First=800;}public static class AccessTools{public static MethodInfo Method(Type t,string name)=>t.GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);}}
namespace UnityEngine {public static class Time{public static float unscaledTime;}}
public static class Time{public static float unscaledTime{get=>UnityEngine.Time.unscaledTime;set=>UnityEngine.Time.unscaledTime=value;}}
public class Obj{public static implicit operator bool(Obj x)=>x!=null;}
public class Info:Obj{public bool Disconnected;public string PlayerName="peer";}
public class PlayerControl:Obj{public static PlayerControl LocalPlayer;public static List<PlayerControl> AllPlayerControls=new();public uint NetId;public int OwnerId;public bool AmOwner;public Info Data=new();}
public enum DisconnectReasons{Custom}
namespace InnerNet{public class InnerNetClient{public enum GameStates{NotJoined,Joined,Started}}}
public class AmongUsClient:Obj{public static AmongUsClient Instance=new();public bool AmHost;public int HostId=10;public InnerNet.InnerNetClient.GameStates GameState=InnerNet.InnerNetClient.GameStates.Joined;public string LastCustomDisconnect;public int Exits;public void ExitGame(DisconnectReasons r){Exits++;}public void CoStartGameClient(){}}
public class LobbyBehaviour{public static bool Instance=true;public void Start(){}}
public class GameStartManager{public void BeginGame(){}}
public class HudManager{public void Update(){}}
public static class TutorialManager{public static bool InstanceExists;}
public static class FakeLog{public static void Warning(string s){}}
namespace TouMarmo{public class TouMarmoPlugin{}}
namespace TouMarmo.Modules{public static class MarmoSession{public static void Notify(string s){}}}
namespace TouMarmo.Modifiers{public class JesterFirstBodyArrowModifier{}}
namespace MiraAPI{public class MiraApiPlugin{}}
namespace MiraAPI.Networking{public readonly struct NetData{public readonly uint Id;public readonly byte[] Data;public NetData(uint id,byte[] data){Id=id;Data=data;}}}
namespace MiraAPI.Networking.Modifiers{public struct ModifierData{public uint TypeId;}}
namespace MiraAPI.Modifiers{public static class ModifierManager{public static Type GetModifierType(uint id)=>id==1?typeof(TouMarmo.Modifiers.JesterFirstBodyArrowModifier):typeof(object);}}
namespace MiraAPI.GameOptions{public static class ModdedOptionsManager{public static MiraAPI.Networking.NetData[] Last;public static int Sends;private static void HandleSyncOptions(MiraAPI.Networking.NetData[] data){Last=data;}private static void SyncAllOptions(int id){Sends++;}}}
namespace TownOfUs.Networking{public class SendClientModInfoRpc{}}
namespace Reactor.Networking.Rpc{public class Rpc<T>{public static Rpc<T> Instance=new();public static int Sent;public void Send(PlayerControl p,Dictionary<byte,string> d){Sent++;}}}
namespace Il2CppSystem.Collections{public interface IEnumerator{}public class ArrayList{class Empty:IEnumerator{}public IEnumerator GetEnumerator()=>new Empty();}}
'@
foreach($file in @('BuildCompatibility','ClientBuildGate')){
 $part=Get-Content (Join-Path $Root "Modules/$file.cs") -Raw
 $part=[regex]::Replace($part,'(?m)^using [^\r\n]+;\r?\n','')
 $code+=$part.Replace('namespace TouMarmo.Modules;','namespace TouMarmo.Modules {')+'}'
}
$code+=@'
public static class Tests{
 static int n;static void Check(bool b,string why){n++;if(!b)throw new Exception(why);}
 static void Reset(){TouMarmo.Modules.BuildCompatibility.Clear();TouMarmo.Modules.ClientBuildGate.Reset();AmongUsClient.Instance.Exits=0;AmongUsClient.Instance.AmHost=false;TutorialManager.InstanceExists=false;Time.unscaledTime=0;MiraAPI.GameOptions.ModdedOptionsManager.Last=null;}
 public static int Run(){
  var client=AmongUsClient.Instance;var host=new PlayerControl{OwnerId=10,NetId=1};var local=new PlayerControl{OwnerId=20,NetId=2,AmOwner=true};PlayerControl.LocalPlayer=local;PlayerControl.AllPlayerControls.AddRange(new[]{host,local});Reset();
  Check(!TouMarmo.Modules.ClientBuildGate.Trusted,"Host unverified by default");
  var bytes=new byte[]{7};Check(!TouMarmo.Modules.ClientBuildGate.Options(new[]{new MiraAPI.Networking.NetData(2,bytes)}),"Unverified settings not interpreted");bytes[0]=99;
  TouMarmo.Modules.ClientBuildGate.Tick();Check(client.Exits==0,"Verification has a grace period");
  TouMarmo.Modules.BuildCompatibility.Record(local,new(){{250,TouMarmo.Modules.BuildCompatibility.Manifest}});TouMarmo.Modules.ClientBuildGate.Receive(local);Check(!TouMarmo.Modules.ClientBuildGate.Trusted,"Local fingerprint cannot verify remote host");
  TouMarmo.Modules.BuildCompatibility.Record(host,new(){{250,TouMarmo.Modules.BuildCompatibility.Manifest}});TouMarmo.Modules.ClientBuildGate.Receive(host);
  Check(TouMarmo.Modules.ClientBuildGate.Trusted,"Matching host verified");Check(MiraAPI.GameOptions.ModdedOptionsManager.Last.Length==1&&MiraAPI.GameOptions.ModdedOptionsManager.Last[0].Data[0]==7,"Pending chunk preserved and deep-copied before verification");
  Check(TouMarmo.Modules.ClientBuildGate.Options(new[]{new MiraAPI.Networking.NetData(3,new byte[]{8})}),"Verified settings accepted normally");
  Il2CppSystem.Collections.IEnumerator empty=null;Check(TouMarmo.Modules.ClientBuildGateStart.Prefix(ref empty),"Matching client can start");
  Reset();TouMarmo.Modules.ClientBuildGate.Tick();Time.unscaledTime=13;TouMarmo.Modules.ClientBuildGate.Tick();Check(client.Exits==1,"Missing host handshake times out");TouMarmo.Modules.ClientBuildGate.Tick();Check(client.Exits==1,"No repeated disconnects");
  Reset();TouMarmo.Modules.BuildCompatibility.Record(host,new(){{250,"old build"}});TouMarmo.Modules.ClientBuildGate.Receive(host);Check(client.Exits==1,"Explicit mismatch disconnects immediately");Check(MiraAPI.GameOptions.ModdedOptionsManager.Last==null,"Wrong settings never replayed");
  Reset();Check(!TouMarmo.Modules.ClientBuildGateStart.Prefix(ref empty)&&empty!=null&&client.Exits==1,"Early game start blocked with valid empty coroutine");
  Reset();client.AmHost=true;Check(TouMarmo.Modules.ClientBuildGate.Trusted,"Host unaffected");TouMarmo.Modules.BuildCompatibility.Record(local,new(){{250,TouMarmo.Modules.BuildCompatibility.Manifest}});local.AmOwner=false;TouMarmo.Modules.ClientBuildGate.Receive(local);Check(MiraAPI.GameOptions.ModdedOptionsManager.Sends==1,"Verified new client receives fresh settings after host acknowledgement");local.AmOwner=true;
  Reset();TutorialManager.InstanceExists=true;Check(TouMarmo.Modules.ClientBuildGate.Trusted,"Offline Practice bypasses lobby handshake");
  Check(!TouMarmo.Modules.ClientBuildModifierGate.Prefix(new MiraAPI.Networking.Modifiers.ModifierData{TypeId=1}),"Local-only Jester arrow rejected from network");
  Check(TouMarmo.Modules.ClientBuildModifierGate.Prefix(new MiraAPI.Networking.Modifiers.ModifierData{TypeId=2}),"Other modifiers retain normal handling");
  TutorialManager.InstanceExists=false;return n;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([Tests]::Run()) production client verification/RPC gate checks with API doubles; not a live multiplayer test."
