param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
function Method([string]$text,[string]$name){
 $match=[regex]::Match($text,'(?m)^ *(?:public|private) static [\w.<>?]+ '+[regex]::Escape($name));if(!$match.Success){throw "Missing method $name"};$start=$match.Index
 $open=$text.IndexOf('{',$start);$end=$open+1;$depth=1
 while($depth -gt 0){if($text[$end] -eq '{'){$depth++};if($text[$end] -eq '}'){$depth--};$end++}
 $text.Substring($start,$end-$start)
}
$s=Get-Content (Join-Path $Root 'Modules/Sweets/SweetsRuntime.cs') -Raw
$a=$s.IndexOf('    public sealed class Cycle');$b=$s.IndexOf('    public static HashSet<byte> Flowers')
$state=$s.Substring($a,$b-$a)
$methods=@('CanGive(','CanPollinate(','RpcGiveFlower(','RpcPollinate(') | ForEach-Object {Method $s $_}
$m=Get-Content (Join-Path $Root 'Modules/Manipulator/ManipulatorRuntime.cs') -Raw
$wait=(Method $m 'WaitForMenu(PlayerControl').Replace('private static','public static')
$code=@'
using System;using System.Linq;using System.Collections;using System.Collections.Generic;
public class PlayerControl {public static PlayerControl LocalPlayer;public byte PlayerId;public bool Alive=true,Sweets=true,Impostor,CanMove=true;public float X;public bool IsImpostorAligned()=>Impostor;public float GetTruePosition()=>X;public static implicit operator bool(PlayerControl p)=>p!=null;}
public static class Time {public static float time,unscaledTime;}
public static class Vector2 {public static float Distance(float a,float b)=>Math.Abs(a-b);}
public static class MarmoSession {public static bool InRound=true;public static bool Alive(PlayerControl p)=>p!=null&&p.Alive;}
public static class Minigame {public static bool Instance;}
public static class BigBrainRuntime {public static void Record(PlayerControl a,PlayerControl b,bool failed){}}
public static class Log {public static void LogInfo(string s){}}
public class Option {public float Value=7;}
public class Settings {public Option FlowerCooldown=new();}
public static class Sweets {
 public static Settings Options=new();public static int RequiredFlowers=3;public static float Distance=2.25f;public static bool IsSweets(PlayerControl p)=>p!=null&&p.Sweets;
'@
$code+=$state+[string]::Join("`n",$methods)+'}'
$code+='public static class Manip {public static int Requests;public static bool CanStart(PlayerControl p)=>MarmoSession.InRound&&p.Alive&&p.CanMove;public static void RpcRequest(PlayerControl p,PlayerControl t){Requests++;}'+$wait+'}'
$code+=@'
public static class RoleUiTests {
 static int checks;static void Check(bool b,string why){checks++;if(!b)throw new Exception(why);}
 public static int Run(){
  var a=new PlayerControl{PlayerId=1};var b=new PlayerControl{PlayerId=2};var t=new PlayerControl{PlayerId=3};PlayerControl.LocalPlayer=a;
  var pending=Manip.WaitForMenu(t);Minigame.Instance=true;a.CanMove=false;
  Check(pending.MoveNext()&&Manip.Requests==0,"Selection always waits one frame");
  Check(pending.MoveNext()&&Manip.Requests==0,"Open tablet cannot send request");
  Minigame.Instance=false;Check(pending.MoveNext()&&Manip.Requests==0,"Closing tablet waits for movement restoration");
  a.CanMove=true;Check(!pending.MoveNext()&&Manip.Requests==1,"Request sent after tablet really closes");
  pending=Manip.WaitForMenu(t);pending.MoveNext();MarmoSession.InRound=false;pending.MoveNext();Check(Manip.Requests==1,"Meeting/cancelled round cannot send stale request");MarmoSession.InRound=true;
  pending=Manip.WaitForMenu(t);pending.MoveNext();t.Alive=false;pending.MoveNext();Check(Manip.Requests==1,"Dead selected target rejected");t.Alive=true;
  Check(Sweets.CanGive(a,t),"Valid flower recipient accepted");Sweets.RpcGiveFlower(a,t);
  Check(Sweets.For(a).FlowersGiven==1&&Sweets.For(a).Flowers.Count==1,"Accepted flower updates count and holder");
  Check(!Sweets.CanGive(a,t)&&!Sweets.CanPollinate(a),"Cooldown/duplicate cannot spend another flower");
  Check(Sweets.For(b).FlowersGiven==0&&Sweets.For(b).ReadyAt==0,"Second Sweets has independent state and timer");
  Sweets.RpcGiveFlower(b,t);Check(Sweets.For(b).FlowersGiven==1,"Other Sweets may independently give same recipient a flower");
  Time.time=10;var u=new PlayerControl{PlayerId=4};Sweets.RpcGiveFlower(a,u);var v=new PlayerControl{PlayerId=5};Sweets.RpcGiveFlower(a,v);Time.time=20;
  Check(Sweets.CanPollinate(a),"Completed distribution unlocks Pollinate");Sweets.RpcPollinate(a);
  Check(Sweets.For(a).FlowersGiven==0&&Sweets.For(a).Flowers.Count==0&&!Sweets.CanPollinate(a),"Pollinate clears completed cycle");
  Check(Sweets.For(b).FlowersGiven==1&&Sweets.For(b).Flowers.Count==1,"Pollinate does not clear another Sweets");
  Time.time=30;Check(Sweets.CanGive(a,t),"Next Flower cycle is available");
  t.Impostor=true;Check(!Sweets.CanGive(a,t),"Impostor cannot receive flower");t.Impostor=false;t.Alive=false;Check(!Sweets.CanGive(a,t),"Dead player rejected");t.Alive=true;t.X=20;Check(!Sweets.CanGive(a,t),"Out-of-range recipient rejected");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([RoleUiTests]::Run()) production menu-wait and Sweets cycle assertions with API doubles."
