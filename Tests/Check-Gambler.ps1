param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
# Execute the production postfix against minimal API doubles. This does not emulate Unity or networking.
$code=@'
#nullable enable annotations
using System; using System.Linq; using System.Collections.Generic;
using HarmonyLib; using MiraAPI.GameEnd; using TownOfUs.GameOver;
using TownOfUs.Modules; using TownOfUs.Roles.Neutral; using TouMarmo.Roles;
namespace HarmonyLib {
 public class HarmonyPatch:Attribute { public HarmonyPatch(Type t,string n){} }
 public class HarmonyPostfix:Attribute{}
 public class HarmonyPriority:Attribute{public HarmonyPriority(int n){}}
 public static class Priority{public const int Last=0;}
}
namespace MiraAPI.GameEnd {public static class CustomGameOver { public static int GameOverReason<T>()=>typeof(T)==typeof(TownOfUs.GameOver.DrawGameOver)?90:91;}}
namespace TownOfUs.GameOver {public class DrawGameOver{} public class HostGameOver{}}
namespace TownOfUs.Modules {public static class Extensions {public static Role GetRoleWhenAlive(this PlayerControl p)=>p.Data.Role;}}
namespace TownOfUs.Roles.Neutral {public class NeutralRole:Role{public NeutralRole(){Neutral=true;}}}
namespace TouMarmo.Roles {public class GamblerRole:TownOfUs.Roles.Neutral.NeutralRole {public bool HasWon;}}
namespace TownOfUs.Patches {public static class EndGamePatches{public static int Builds;public static void BuildEndGameData(){Builds++;}}}
public class UnityObject {public static implicit operator bool(UnityObject o)=>o!=null;}
public class Role:UnityObject {public bool Neutral;public int TypeId;public bool IsNeutral()=>Neutral;}
public class RoleManager:UnityObject {
 public static RoleManager Instance=new RoleManager(); public Dictionary<int,Role> Roles=new Dictionary<int,Role>();
 public Role GetRole(int id)=>Roles.TryGetValue(id,out var role)?role:null;
}
public class GameData:UnityObject {public static GameData Instance=new GameData();}
public class Outfit {public int ColorId;}
public class PlayerData:UnityObject {public string PlayerName;public bool Disconnected,IsDead;public Outfit DefaultOutfit=new Outfit();public Role Role;}
public class PlayerControl:UnityObject {public static List<PlayerControl> AllPlayerControls=new List<PlayerControl>();public PlayerData Data;}
public class CachedPlayerData {
 public string PlayerName;public int ColorId,RoleWhenAlive;
 public CachedPlayerData(PlayerData p){PlayerName=p.PlayerName;ColorId=p.DefaultOutfit.ColorId;RoleWhenAlive=p.Role.TypeId;}
}
public class EndGameResult {public int GameOverReason;public static List<CachedPlayerData> CachedWinners=new List<CachedPlayerData>();}
public class AmongUsClient {public void OnGameEnd(){}}
'@
foreach($path in @('Modules/FeedbackRules.cs','Patches/GamblerSharedVictoryPatch.cs')){
 $part=Get-Content (Join-Path $Root $path) -Raw
 $part=[regex]::Replace($part,'(?m)^using [^\r\n]+;\r?\n','')
 $part=[regex]::Replace($part,'namespace ([\w.]+);','namespace $1 {')
 $code+=$part+'}'
}
$code+=@'
public static class GamblerTests {
 static int checks;
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
 static PlayerControl Setup(bool neutral=true,bool goal=false,bool disconnected=false,bool dead=false){
  RoleManager.Instance=new RoleManager(); GameData.Instance=new GameData();
  EndGameResult.CachedWinners.Clear();PlayerControl.AllPlayerControls.Clear();TownOfUs.Patches.EndGamePatches.Builds=0;
  var winner=new Role{Neutral=neutral,TypeId=1};RoleManager.Instance.Roles[1]=winner;
  EndGameResult.CachedWinners.Add(new CachedPlayerData(new PlayerData{PlayerName="Winner",Role=winner}));
  var gambler=new TouMarmo.Roles.GamblerRole{TypeId=2,HasWon=goal};RoleManager.Instance.Roles[2]=gambler;
  var p=new PlayerControl{Data=new PlayerData{PlayerName="Gambler",Role=gambler,Disconnected=disconnected,IsDead=dead}};
  PlayerControl.AllPlayerControls.Add(p);return p;
 }
 static void End(int reason=1)=>TouMarmo.Patches.GamblerSharedVictoryPatch.Postfix(new EndGameResult{GameOverReason=reason});
 public static int Run(){
  var p=Setup();End();Check(EndGameResult.CachedWinners.Count==2,"Neutral victory grants unearned co-win");
  Check(((TouMarmo.Roles.GamblerRole)p.Data.Role).HasWon,"Personal result updated");
  Check(EndGameResult.CachedWinners[0].PlayerName=="Winner","Original winner retained");
  Check(TownOfUs.Patches.EndGamePatches.Builds==1,"Summary rebuilt after adding winner");
  End();Check(EndGameResult.CachedWinners.Count==2,"Repeated end does not duplicate gambler");
  Check(TownOfUs.Patches.EndGamePatches.Builds==1,"Repeated end does not rebuild unnecessarily");
  Setup(false);End();Check(EndGameResult.CachedWinners.Count==1,"Crew/impostor victory alone grants no bonus");
  Setup(true,true);End();Check(EndGameResult.CachedWinners.Count==2,"Completed goal remains compatible");
  Setup(true,false,true);End();Check(EndGameResult.CachedWinners.Count==1,"Disconnected gambler excluded");
  Setup(true,false,false,true);End();Check(EndGameResult.CachedWinners.Count==2,"Dead connected gambler can share victory");
  Setup();End(90);Check(EndGameResult.CachedWinners.Count==1,"Draw grants no bonus");
  Setup();End(91);Check(EndGameResult.CachedWinners.Count==1,"Host cancellation grants no bonus");
  p=Setup();EndGameResult.CachedWinners.Clear();EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
  End();Check(!((TouMarmo.Roles.GamblerRole)p.Data.Role).HasWon,"Gambler alone cannot recursively create bonus win");
  Setup();EndGameResult.CachedWinners.Clear();End();Check(EndGameResult.CachedWinners.Count==0,"No premature winner from empty result");
  Setup();RoleManager.Instance=null;End();Check(EndGameResult.CachedWinners.Count==1,"Missing role manager safely ignored");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([GamblerTests]::Run()) production postfix checks with API doubles; no Unity/multiplayer emulation."
