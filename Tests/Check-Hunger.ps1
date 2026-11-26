param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$progress=(Get-Content (Join-Path $Root 'Modules/Hunger/HungerProgress.cs') -Raw).Replace('namespace TouMarmo.Modules.Hunger;','namespace TouMarmo.Modules.Hunger {')+'}'
$events=Get-Content (Join-Path $Root 'Modules/Hunger/HungerEvents.cs') -Raw
$start=$events.IndexOf('    public static void Meeting(');$end=$events.IndexOf('    [MethodRpc', $start)
$rpc=$events.Substring($events.IndexOf('    public static void RpcProgress('))
$speed=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Patches/PlayerSpeedPatch.cs') -Raw
$s=$speed.IndexOf('            if (!pc.HasDied() && pc.TryGetModifier<TouMarmo.Modifiers.HungerModifier>');$e=$speed.IndexOf(';',$s)
$speedClause=$speed.Substring($s,$e-$s+1)
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;using TouMarmo.Modules.Hunger;using TouMarmo.Modifiers;
public class Obj {public static implicit operator bool(Obj? o)=>o!=null;}
public class Info:Obj {public bool IsDead;}
public class PlayerControl:Obj {public static PlayerControl LocalPlayer=new(){Host=true};public bool Host,Dead;public byte PlayerId=1;public Info Data=new();public TouMarmo.Modifiers.HungerModifier? Hunger;public bool IsHost()=>Host;public T? GetModifier<T>() where T:class=>Hunger as T;public bool TryGetModifier<T>(out T value) where T:class {value=(Hunger as T)!;return value!=null;}public bool HasDied()=>Dead;}
namespace TouMarmo.Modifiers {public class HungerModifier {public HungerProgress Progress=new();}}
public static class MarmoSession {public static bool IsHost=true;public static bool Alive(PlayerControl? p)=>p!=null&&!p.Dead;}
public class MeetingHud {public byte reporterId=1;public int Id=1;public int GetInstanceID()=>Id;}
public class StartMeetingEvent {public MeetingHud MeetingHud=new();}
public class MeetingRoomManager {public static MeetingRoomManager? Instance=new();public PlayerControl reporter=new(){Hunger=new()};public Info? target=new(){IsDead=true};}
public class Number {public float Value=.25f;}
public class UniversalModifiersOptions {public Number HungerSpeedPerReport=new();}
public static class OptionGroupSingleton<T> where T:new() {public static T Instance=new();}
public static class Handler {
'@
$code+=$events.Substring($start,$end-$start)+$rpc
$code+='public static class Speed {public static float Run(PlayerControl pc,float initial){var __result=initial;'+$speedClause+'return __result;}}'+$progress
$code+=@'
public static class Tests {
static int n;static void Check(bool ok,string name){n++;if(!ok)throw new Exception(name);}
public static int Run(){
 var p=new HungerProgress();Check(p.Reports==0&&p.Bonus==0,"New modifier has no bonus");
 Check(p.Record(1,.25f)&&p.Bonus==.25f,"First body bonus");Check(!p.Record(1,.25f)&&p.Bonus==.25f,"Duplicate meeting ignored");Check(p.Record(2,.25f)&&p.Bonus==.5f,"Stack across meetings");Check(p.Record(3,.5f)&&p.Bonus==1f,"Configured increment respected");Check(!p.Record(4,-1)&&!p.Record(4,float.NaN)&&!p.Record(4,float.PositiveInfinity),"Invalid options rejected");Check(p.Record(4,0)&&p.Reports==4,"Zero bonus setting counts report without speed");
 var client=new HungerProgress();Check(client.Apply(3,.75f),"Absolute host state applied");Check(!client.Apply(2,.5f)&&!client.Apply(3,.75f),"Duplicate/out of order RPC ignored");Check(!client.Apply(4,float.NaN)&&!client.Apply(4,-1),"Invalid network values rejected");Check(client.Apply(5,1.25f)&&client.Bonus==1.25f,"Missed earlier RPC recovered by absolute state");Check(new HungerProgress().Reports==0,"Next game's modifier starts fresh");
 var room=MeetingRoomManager.Instance!;var owner=room.reporter;var ev=new StartMeetingEvent();Handler.Meeting(ev);Check(owner.Hunger!.Progress.Reports==1&&owner.Hunger.Progress.Bonus==.25f,"Accepted host body meeting awards reporter");Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Duplicate accepted event ignored");
 ev.MeetingHud.Id=2;room.target=null;Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Emergency never awards speed");room.target=new(){IsDead=false};Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Living target not a body");room.target.IsDead=true;
 MarmoSession.IsHost=false;Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Client cannot award reports");MarmoSession.IsHost=true;
 owner.Dead=true;Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Dead reporter not awarded");owner.Dead=false;
 ev.MeetingHud.reporterId=2;Handler.Meeting(ev);Check(owner.Hunger.Progress.Reports==1,"Reporter identity must match accepted meeting");ev.MeetingHud.reporterId=1;
 OptionGroupSingleton<UniversalModifiersOptions>.Instance.HungerSpeedPerReport.Value=.5f;Handler.Meeting(ev);Check(owner.Hunger.Progress.Bonus==.75f,"Second meeting uses host setting and retains previous bonus");
 var remote=new PlayerControl{Hunger=new()};Handler.RpcProgress(new PlayerControl(),remote,3,.75f);Check(remote.Hunger.Progress.Reports==0,"Forged nonhost RPC rejected");Handler.RpcProgress(PlayerControl.LocalPlayer,remote,3,.75f);Check(remote.Hunger.Progress.Reports==3,"Host updates other clients' modifier state");Handler.RpcProgress(PlayerControl.LocalPlayer,remote,2,.5f);Check(remote.Hunger.Progress.Reports==3,"Late state cannot roll back speed");
 Check(Speed.Run(remote,1)==1.75f,"Speed is additive in x units");Check(Speed.Run(remote,1.5f)==2.25f,"Configured base speed preserved");remote.Dead=true;Check(Speed.Run(remote,1)==1,"No extra ghost speed");Check(Speed.Run(new PlayerControl(),1)==1,"Unmodified players unchanged");
 return n;
}}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
"PASS $([Tests]::Run()) production Hunger progress, meeting, host RPC and speed assertions with API doubles."
$modifier=Get-Content (Join-Path $Root 'Modifiers/HungerModifier.cs') -Raw
if($modifier -notmatch 'IsGuessable => false' -or $modifier -notmatch 'HideFromGuessing => true'){throw 'Hunger guessing must be disabled'}
if($modifier -notmatch 'ModifierFaction.UniversalPassive' -or $modifier -notmatch 'role is not SpectatorRole'){throw 'Hunger must be available to all playing teams'}
if($events -match 'Meeting\(ReportBodyEvent'){throw 'Cancelled reports must not award stacks'}
if($speed.IndexOf('hunger.Progress.Bonus') -gt $speed.IndexOf('freeze.SpeedFactor')){throw 'Freeze must still apply after hunger bonus'}
'PASS universal assignment, unguessability and freeze-order wiring; no game or multiplayer launched.'
