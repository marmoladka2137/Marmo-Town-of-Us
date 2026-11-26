param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$code="#nullable enable annotations`nusing System; using System.Linq; using System.Collections.Generic;"
foreach($name in @('FeedbackRules','AbilityLimits')){
 $part=Get-Content (Join-Path $Root "Modules/$name.cs") -Raw -Encoding UTF8
 $part=$part.Replace('namespace TouMarmo.Modules;','namespace TouMarmo.Modules {')
 $part=[regex]::Replace($part,'public static int Count\(MiraAPI[^\r\n]+','')
 $code+=$part+'}'
}
$code+=@'
public static class FeedbackTests {
 static int count;
 static void Check(bool value,string name){if(!value)throw new Exception(name);count++;}
 public static int Run(){
  Check(!TouMarmo.Modules.FeedbackRules.ShieldFlash(false,false),"Protected player or spectator receives no flash");
  Check(TouMarmo.Modules.FeedbackRules.ShieldFlash(true,false),"Attacker receives flash");
  Check(TouMarmo.Modules.FeedbackRules.ShieldFlash(false,true),"Provider receives flash");
  Check(TouMarmo.Modules.FeedbackRules.ShieldFlash(true,true),"Overlapping attacker/provider gets a single entitlement");
  var names=TouMarmo.Modules.FeedbackRules.UniqueRoles(new string[]{"Medic","Medic"," medic ",null,"", "Warden","WARDEN"});
  Check(names.SequenceEqual(new[]{"Medic","Warden"}),"Feedback deduplicates role names by case and whitespace");
  Check(TouMarmo.Modules.FeedbackRules.UniqueRoles(new string[]{null," "}).Length==0,"Empty detections produce no names");
  Check(TouMarmo.Modules.AbilityLimits.Count(0)==int.MaxValue,"Zero selects unlimited integer capacity");
  Check(TouMarmo.Modules.AbilityLimits.Count(0f)==int.MaxValue,"Zero selects unlimited option capacity");
  Check(TouMarmo.Modules.AbilityLimits.Count(-1f)==int.MaxValue,"Existing minus-one infinity remains unlimited");
  Check(TouMarmo.Modules.AbilityLimits.Count(3f)==3,"Finite option capacity unchanged");
  Check(TouMarmo.Modules.AbilityLimits.Count(int.MaxValue)==int.MaxValue,"Already-normalized capacity does not overflow");
  Check(TouMarmo.Modules.FeedbackRules.CorpseKills(0,true,false)==0,"Blocked corpse attack does not consume a kill");
  Check(TouMarmo.Modules.FeedbackRules.CorpseKills(0,true,true)==1,"Successful pending corpse attack consumes one kill");
  Check(TouMarmo.Modules.FeedbackRules.CorpseKills(1,false,true)==1,"Duplicate completion does not consume a second kill");
  Check(TouMarmo.Modules.FeedbackRules.GamblerSharedWin(true,false),"Gambler shares any confirmed neutral victory without requiring a goal");
  Check(!TouMarmo.Modules.FeedbackRules.GamblerSharedWin(false,false),"No bonus victory if no neutral won");
  Check(!TouMarmo.Modules.FeedbackRules.GamblerSharedWin(true,true),"Cancellation does not create a co-winner");
  Check(!TouMarmo.Modules.FeedbackRules.GamblerSharedWin(false,true),"Cancelled empty result has no co-winner");
  for(int n=1;n<=100;n++)Check(TouMarmo.Modules.AbilityLimits.Count(n)==n,"Finite capacity preserved");
  return count;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([FeedbackTests]::Run()) protection audience, distinct-role feedback and capacity checks."
