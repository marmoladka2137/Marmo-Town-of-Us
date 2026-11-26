param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Modules/BuildCompatibility.cs') -Raw
$part=[regex]::Replace($part,'(?m)^using [^\r\n]+;\r?\n','')
$part=$part.Replace('namespace TouMarmo.Modules;','namespace TouMarmo.Modules {')
$code=@'
#nullable enable annotations
using System; using System.Linq; using System.Collections.Generic; using HarmonyLib; using MiraAPI; using static FakeLog;
namespace HarmonyLib {public class HarmonyPatch:Attribute{public HarmonyPatch(Type type,string method){}}public class HarmonyPrefix:Attribute{}public class HarmonyPriority:Attribute{public HarmonyPriority(int v){}}public static class Priority{public const int First=800;}}
namespace MiraAPI {public class MiraApiPlugin{}}
namespace TouMarmo {public class TouMarmoPlugin{}}
public class Obj{public static implicit operator bool(Obj o)=>o!=null;}
public class PlayerData:Obj {public bool Disconnected;public string PlayerName="Player";}
public class PlayerControl:Obj {public static List<PlayerControl> AllPlayerControls=new();public PlayerData Data=new();public uint NetId;public bool AmOwner;}
public class AmongUsClient:Obj {public static AmongUsClient Instance=new();public bool AmHost=true;}
public static class TutorialManager {public static bool InstanceExists;}
public class GameStartManager {public void BeginGame(){}}
public class LobbyBehaviour {public void Start(){}}
public static class FakeLog {public static void Warning(string s){}}
namespace TouMarmo.Modules {public static class MarmoSession {public static string Message="";public static void Notify(string s){Message=s;}}}
'@
$code+=$part+'}'
$code+=@'
public static class BuildTests {
 static int n;static void Check(bool value,string why){if(!value)throw new Exception(why);n++;}
 public static int Run(){
  TouMarmo.Modules.BuildCompatibility.Clear();
  PlayerControl.AllPlayerControls.Add(new PlayerControl{NetId=1,AmOwner=true});
  var peer=new PlayerControl{NetId=2};PlayerControl.AllPlayerControls.Add(peer);
  Check(!TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Unverified peer cannot start");
  Check(TouMarmo.Modules.MarmoSession.Message.Contains("same MTOU"),"Host receives reason");
  TouMarmo.Modules.BuildCompatibility.Record(peer,new Dictionary<byte,string>());
  Check(!TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Legacy peer without fingerprint rejected");
  TouMarmo.Modules.BuildCompatibility.Record(peer,new Dictionary<byte,string>{{250,"another build"}});
  Check(!TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Same visible version is insufficient");
  TouMarmo.Modules.BuildCompatibility.Record(peer,new Dictionary<byte,string>{{250,TouMarmo.Modules.BuildCompatibility.Manifest}});
  Check(TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Verified identical peer allowed");
  TouMarmo.Modules.BuildCompatibility.Clear();
  Check(!TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"New lobby clears stale verification");
  peer.Data.Disconnected=true;Check(TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Disconnected player does not block");
  peer.Data.Disconnected=false;AmongUsClient.Instance.AmHost=false;
  Check(TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Only host enforces start");
  AmongUsClient.Instance.AmHost=true;TutorialManager.InstanceExists=true;
  Check(TouMarmo.Modules.BuildCompatibilityStartPatch.Prefix(),"Offline Practice unaffected");
  Check(!TouMarmo.Modules.BuildCompatibility.Matches(null,null),"Missing manifests never match");
  Check(!TouMarmo.Modules.BuildCompatibility.Matches("A","a"),"Fingerprint comparison exact");
  Check(TouMarmo.Modules.BuildCompatibility.Matches("A","A"),"Identical fingerprint matches");
  return n;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([BuildTests]::Run()) production compatibility gate checks with API doubles; not multiplayer."
