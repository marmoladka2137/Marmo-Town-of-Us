param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$source=Get-Content -Raw (Join-Path $Root 'Modules/Jester/JesterExileState.cs')
$source="#nullable enable`n"+'using System; using System.Collections.Generic; '+$source.Replace('namespace TouMarmo.Modules.Jester;','namespace TouMarmo.Modules.Jester {')+'}'
$source+=@'
public static class JesterExileTests {
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 public static int Run(){
  foreach(bool jester in new[]{false,true})foreach(bool alive in new[]{false,true})foreach(bool tie in new[]{false,true})foreach(bool target in new[]{false,true})foreach(bool ends in new[]{false,true}){
   var s=new TouMarmo.Modules.Jester.JesterExileState();
   s.Capture(100,0,target?(byte?)7:null,888,jester,alive,tie,ends);
   Check(!s.HasWon(7),"No premature victory before actual ejection");
   Check(!s.Confirm(8),"Chain death of other player does not count");
   var qualifies=target&&jester&&alive&&!tie;
   Check(s.Confirm(7)==qualifies,"Only living Jester voted out qualifies; no skip or tie");
   Check(s.HasWon(7)==qualifies && s.EndsGame(7)==(qualifies&&ends),"Host end-game mode honored");
   Check(!s.Confirm(7),"No duplicate victory");
   s.BeginMeeting();Check(s.HasWon(7)==qualifies,"Winner preserved through subsequent meeting");
   s.Clear();Check(!s.HasWon(7),"No leakage into next game");
  }
  var state=new TouMarmo.Modules.Jester.JesterExileState();
  Check(!state.Confirm(7),"Normal kill/special kill without ballot cannot win");
  state.Capture(1,0,7,888,true,true,false,true);
  state.Capture(1,0,7,999,false,false,false,false);
  Check(state.Confirm(7),"Duplicate result cannot replace captured role with Spectre");
  Check(state.TryGet(7,out var receipt)&&receipt.RoleAtVote==888&&receipt.EndsGame,"Immutable original role and settings");
  state.Clear();state.Capture(2,0,7,888,true,true,false,true);
  state.Capture(2,1,8,999,false,true,false,true);
  Check(!state.Confirm(7)&&!state.Confirm(8),"Overruled ballot replaces pending candidate");
  state.Capture(3,0,7,888,true,true,false,false);state.BeginMeeting();
  Check(!state.Confirm(7),"Cancelled prior meeting cannot produce stale victory");
  state.Capture(4,0,7,888,true,true,false,false);Check(state.Confirm(7)&&!state.EndsGame(7),"Haunts/Nothing record winner without ending game");
  state.BeginMeeting();state.Capture(5,0,9,888,true,true,false,true);state.Confirm(9);
  Check(state.HasWon(7)&&state.HasWon(9),"Multiple Jest winners retained across rounds");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $source
Write-Output "PASS: $([JesterExileTests]::Run()) Jester ballot/exile assertions; no Unity or network transport."
