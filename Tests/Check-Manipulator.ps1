param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Modules/Manipulator/ManipulationClock.cs') -Raw
$part=$part.Replace('namespace TouMarmo.Modules.Manipulator;','namespace TouMarmo.Modules.Manipulator {')
$code='using System;'+$part+'}'
$code+=@'
public static class ManipulatorClockTests {
 static int n;static void Check(bool ok,string why){n++;if(!ok)throw new Exception(why);}
 public static int Run(){
  var c=new TouMarmo.Modules.Manipulator.ManipulationClock(20);
  Check(!c.Advance(10,true)&&c.Remaining==20,"Meeting pauses obligation");
  Check(!c.Advance(19,false)&&c.Remaining==1,"No early deadline");
  Check(c.Advance(1,false)&&c.Remaining==0,"Deadline reached");
  c.Resolve();Check(c.Resolved&&!c.Finished&&c.Tail==3,"Three-second observation tail");
  c.Advance(2,false);c.Resolve();Check(c.Tail==1,"Duplicate result does not restart tail");
  c.Advance(10,true);Check(c.Tail==1,"Meeting pauses retained observation");
  c.Advance(1,false);Check(c.Finished,"Observation ends on time");
  for(int time=5;time<=90;time++){
   var clock=new TouMarmo.Modules.Manipulator.ManipulationClock(time);
   Check(!clock.Advance(float.NaN,false)&&clock.Remaining==time,"Reject NaN");
   Check(!clock.Advance(-1,false)&&clock.Remaining==time,"Reject negative delta");
   Check(!clock.Advance(time-.5f,false),"No premature punishment");
   Check(clock.Advance(.5f,false),"Exact configured deadline");
   clock.Resolve();Check(!clock.Advance(3,false)&&clock.Finished,"One terminal transition");
  }
  return n;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([ManipulatorClockTests]::Run()) Manipulator clock assertions; not a Unity/network test."
