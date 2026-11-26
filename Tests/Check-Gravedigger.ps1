param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$ledger=Get-Content (Join-Path $Root 'Modules/Gravedigger/BurialLedger.cs') -Raw
$code='using System;using System.Linq;using System.Collections.Generic;'+$ledger.Replace('namespace TouMarmo.Modules.Gravedigger;','namespace TouMarmo.Modules.Gravedigger {')+'}'
$code+=@'
public static class GraveTests {
 static int checks;static void Check(bool b,string name){checks++;if(!b)throw new Exception(name);}
 public static int Run(){
  var s=new TouMarmo.Modules.Gravedigger.BurialLedger();
  Check(!s.Won(true,3),"No early win");Check(!s.Inherit(10,99),"Cannot select an unburied role");
  Check(s.Bury(1,10),"First corpse accepted");Check(!s.Won(true,3),"One body is insufficient");
  Check(!s.Bury(1,20),"Repeated packet/corpse cannot count twice");Check(s.Buried[1]==10,"Role at burial is immutable");
  Check(s.Bury(2,20),"Second corpse accepted");Check(!s.Won(true,3),"Two bodies insufficient");
  Check(s.Bury(3,30)&&s.Won(true,3),"Three corpses win");Check(!s.Won(false,3),"Dead Gravedigger cannot trigger a solo win");
  Check(!s.Won(true,0),"Invalid required count cannot win");
  var inherited=new TouMarmo.Modules.Gravedigger.BurialLedger();inherited.Bury(7,42);inherited.Bury(8,99);
  Check(!inherited.CanInherit(99,99),"Cannot return to Gravedigger");Check(inherited.Inherit(42,99),"Can inherit buried role");
  Check(inherited.Inherited&&inherited.ChosenRole==42,"Permanent role choice recorded");
  Check(!inherited.Inherit(99,99)&&!inherited.Inherit(42,99),"Choice cannot run twice");
  Check(!inherited.Bury(9,43),"Bury removed after inheritance");Check(!inherited.Won(true,1),"Old solo condition removed after inheritance");
  for(int required=1;required<=14;required++){
   var x=new TouMarmo.Modules.Gravedigger.BurialLedger();
   for(byte victim=0;victim<14;victim++){
    x.Bury(victim,10);Check(x.Won(true,required)==(victim+1>=required),"Configured burial threshold");
   }
  }
  var nextGame=new TouMarmo.Modules.Gravedigger.BurialLedger();Check(nextGame.Count==0&&!nextGame.Inherited,"New game has no old identity or counter");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([GraveTests]::Run()) production Gravedigger ledger assertions; no game or multiplayer simulated."
