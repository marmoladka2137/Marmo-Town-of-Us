param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$state=Get-Content -Raw (Join-Path $Root 'Modules/Plumber/VentSpikeState.cs')
$state=$state.Replace('namespace TownOfUs.Modules.Plumber;','namespace TownOfUs.Modules.Plumber {')+'}'
$tests=@'
using System;
using System.Linq;
using System.Collections.Generic;
'@ + $state + @'
public static class SpikeTests {
 static int count;
 static void Check(bool b,string label){count++;if(!b)throw new Exception(label);}
 public static int Run(){
  var host=new TownOfUs.Modules.Plumber.VentSpikeState();
  var peer=new TownOfUs.Modules.Plumber.VentSpikeState();
  byte owner;bool lethal;
  Check(host.Place(1,10,2),"Initial placement");
  Check(!host.Place(1,11,2),"No duplicate trap in one vent");
  Check(host.Place(2,10,2) && !host.Place(3,10,2),"Per-owner cap");
  Check(host.Place(3,11,2),"Independent Plumbers");
  Check(host.Trigger(1,20,3,out owner,out lethal) && owner==10 && !lethal,"First hit and correct owner");
  Check(!host.Trigger(1,20,3,out owner,out lethal),"Trap consumed only once");
  Check(host.Place(1,10,2),"Rearm after consumption");
  Check(!host.Trigger(1,20,3,out owner,out lethal),"Repeated callback during same entry ignored");
  host.Exit(20,1);
  Check(host.Trigger(1,20,3,out owner,out lethal) && !lethal,"Second hit after exit");
  host.Exit(20,1);host.Place(1,10,2);
  Check(host.Trigger(1,20,3,out owner,out lethal) && lethal && host.Hits[20]==0,"Threshold causes one lethal attack and resets hits");
  Check(host.Trigger(2,21,1,out owner,out lethal) && lethal,"Configurable one-hit kill");
  Check(host.Hits[20]==0 && host.Hits[21]==0,"Separate victims");
  host.Clear();Check(host.Traps.Count==0 && host.Hits.Count==0,"New-game cleanup");
  for(int pass=0;pass<20;pass++){
   int vent=pass%4;byte victim=(byte)(30+pass%3);
   Check(host.Place(vent,10,4)==peer.Place(vent,10,4),"Placement replicated");
   byte remoteOwner;bool remoteLethal;
   var a=host.Trigger(vent,victim,3,out owner,out lethal);
   var b=peer.Trigger(vent,victim,3,out remoteOwner,out remoteLethal);
   Check(a==b && owner==remoteOwner && lethal==remoteLethal,"Deterministic remote hit and threshold");
   host.Exit(victim,vent);peer.Exit(victim,vent);
  }
  Check(host.Traps.Count==peer.Traps.Count && host.Hits.All(p=>peer.Hits[p.Key]==p.Value),"Peers agree after repeated cycles");
  host.ClearEntries();peer.ClearEntries();host.Clear();peer.Clear();
  return count;
 }
}
'@
Add-Type -TypeDefinition $tests
Write-Output "PASS: $([SpikeTests]::Run()) vent-spike assertions. This tests replicated state, not live network transport."
