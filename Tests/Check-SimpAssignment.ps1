param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Events/SimpEvents.cs') -Raw
function Extract([string]$signature) {
 $start=$raw.IndexOf($signature);if($start -lt 0){throw $signature};$open=$raw.IndexOf('{',$start);$depth=1;$end=$open+1
 while($depth -gt 0){if($raw[$end] -eq '{'){$depth++};if($raw[$end] -eq '}'){$depth--};$end++};$raw.Substring($start,$end-$start)
}
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;
public class Obj {public static implicit operator bool(Obj? p)=>p!=null;}
public class Role {public bool Imp;public bool IsImpostor()=>Imp;}
public class SpectatorRole:Role{}
public class Data {public Role Role=new();public bool Disconnected;}
public class SimpModifier {public byte TargetId=255;public bool TargetEjected;public string GetDescription()=>"Target";}
public class PlayerControl:Obj {public static List<PlayerControl> AllPlayerControls=new();public byte PlayerId;public Data Data=new();public bool Dead,AmOwner;public SimpModifier? Mod;public bool HasDied()=>Dead;public SimpModifier? GetModifier<T>()=>Mod;}
public class AmongUsClient:Obj {public static AmongUsClient Instance=new();public bool AmHost=true;}
public static class SimpEvents {static void RefreshCooldown(PlayerControl p){}static void Notify(string s){}
'@
$code+=(Extract 'public static void AssignMissingTargets()')+(Extract 'public static void RpcAssign(')+'}'
$code+=@'
public static class Tests {static int n;static void Check(bool v,string name){n++;if(!v)throw new Exception(name);}public static int Run(){
 var owner=new PlayerControl{PlayerId=1,Mod=new(),Data=new Data{Role=new Role{Imp=true}}};PlayerControl.AllPlayerControls.Add(owner);
 SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==255,"No self target when alone");
 PlayerControl.AllPlayerControls.Add(new PlayerControl{PlayerId=2,Dead=true});PlayerControl.AllPlayerControls.Add(new PlayerControl{PlayerId=3,Data=new Data{Disconnected=true}});PlayerControl.AllPlayerControls.Add(new PlayerControl{PlayerId=4,Data=new Data{Role=new SpectatorRole()}});
 SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==255,"No dead disconnected or spectator target");
 var target=new PlayerControl{PlayerId=5};PlayerControl.AllPlayerControls.Add(target);SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==5,"Assign random valid target immediately");
 PlayerControl.AllPlayerControls.Add(new PlayerControl{PlayerId=6});SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==5,"Retries preserve existing target");
 owner.Mod=null;SimpEvents.AssignMissingTargets();owner.Mod=new();SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId is 5 or 6,"Late modifier is assigned without another meeting");
 owner.Mod=new();AmongUsClient.Instance.AmHost=false;SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==255,"Clients do not independently roll targets");
 AmongUsClient.Instance.AmHost=true;owner.Data.Role.Imp=false;SimpEvents.AssignMissingTargets();Check(owner.Mod.TargetId==255,"Non impostor owner excluded");
 return n;}}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
"Simp production assignment checks passed: $([Tests]::Run())"
