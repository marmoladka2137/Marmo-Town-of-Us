param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Modules/Bandit/BanditRuntime.cs') -Raw
function Extract([string]$signature) {
 $start=$raw.IndexOf($signature); if($start -lt 0){throw $signature}; $open=$raw.IndexOf('{',$start);$depth=1;$end=$open+1
 while($depth -gt 0){if($raw[$end] -eq '{'){$depth++};if($raw[$end] -eq '}'){$depth--};$end++}
 $raw.Substring($start,$end-$start)
}
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;
public class RoleBehaviour {public ushort Id;}
public class BanditRole:RoleBehaviour{}
public static class RoleId {public static ushort Get<T>()=>999;}
public class Data {public bool IsDead;public RoleBehaviour Role=new();}
public class PlayerControl {public byte PlayerId; public Data Data=new();public bool FailChange;public static implicit operator bool(PlayerControl? p)=>p!=null;public void ChangeRole(ushort id){if(FailChange)throw new Exception("role failed");Data.Role=new RoleBehaviour{Id=id};}}
public class Snapshot {public int Restores;public void Restore(RoleBehaviour r){Restores++;}}
public class BanditState {public byte Victim=255;public bool Active=>Victim!=255;public bool MeetingSeen,UsedThisRound=true;public ushort VictimRole;public Snapshot? SavedRole;public Action<PlayerControl>? RestoreExternal;public RoleBehaviour? OriginalBandit,OriginalVictim;}
public static class MarmoSession {public static Dictionary<byte,PlayerControl> Players=new();public static PlayerControl? Player(byte id)=>Players.GetValueOrDefault(id);}
public static class Runtime {public static Dictionary<byte,BanditState> States=new();public static bool Restoring;static void RefreshRole(PlayerControl p){} static void RestoreRole(PlayerControl p,ushort id,RoleBehaviour? original)=>p.ChangeRole(id);
'@
$code+=(Extract 'public static bool PromoteVictim(')+(Extract 'public static void Restore(byte id)')+'}'
$code+=@'
public static class Tests {static int n;static void Check(bool v,string name){n++;if(!v)throw new Exception(name);}public static int Run(){
 var bandit=new PlayerControl{PlayerId=1};var victim=new PlayerControl{PlayerId=2};MarmoSession.Players[1]=bandit;MarmoSession.Players[2]=victim;
 Check(!Runtime.PromoteVictim(victim,888),"Ordinary player promotion follows original path");
 var snapshot=new Snapshot();var s=new BanditState{Victim=2,VictimRole=23,SavedRole=snapshot,MeetingSeen=true};Runtime.States[1]=s;
 Runtime.Restore(1);Check(bandit.Data.Role.Id==999,"Bandit restored");Check(victim.Data.Role.Id==23,"Victim restored");Check(snapshot.Restores==1,"Snapshot restored once");Check(!s.Active&&!s.UsedThisRound,"Lease released for next round");Runtime.Restore(1);Check(snapshot.Restores==1,"Repeated return idempotent");
 s.Victim=2;s.VictimRole=23;s.SavedRole=snapshot;s.RestoreExternal=p=>throw new Exception("Old callback must not run");
 Check(Runtime.PromoteVictim(victim,888),"Borrowed Crewpostor promoted");Check(victim.Data.Role.Id==888,"Promotion applies now");Check(s.VictimRole==888&&s.SavedRole==null&&s.RestoreExternal==null,"Old crew role cannot overwrite promotion");
 Runtime.Restore(1);Check(victim.Data.Role.Id==888&&bandit.Data.Role.Id==999,"Promotion survives role return");Check(!Runtime.Restoring,"Guard released");
 s.Victim=2;victim.FailChange=true;try{Runtime.PromoteVictim(victim,888);}catch{}Check(!Runtime.Restoring,"Guard released on exception");victim.FailChange=false;
 return n;}}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
"Bandit production transition checks passed: $([Tests]::Run())"
