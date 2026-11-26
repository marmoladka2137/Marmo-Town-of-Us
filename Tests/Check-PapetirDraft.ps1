param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$ledger=(Get-Content (Join-Path $Root 'Modules/Papetir/PaperLedger.cs') -Raw).Replace('namespace TouMarmo.Modules.Papetir;','namespace TouMarmo.Modules.Papetir {')+'}'
$applier=(Get-Content (Join-Path $Root 'IntegratedTownOfUs/Modules/DraftMode/DraftApplier.cs') -Raw).Replace('using AmongUs.GameOptions;','')
$layout=Get-Content (Join-Path $Root 'Patches/ExpandedLobbyPatches.cs') -Raw
$a=$layout.IndexOf('public static class ExpandedMeetingLayoutPatch');$b=$layout.IndexOf('// Once the palette',$a)
$layout=$layout.Substring($a,$b-$a).Replace('[HarmonyPostfix,HarmonyPriority(Priority.Last)]','')
$code=@'
using System;using System.Linq;using System.Collections.Generic;using TouMarmo.Modules.Papetir;using TownOfUs.Modules.DraftMode;
public enum RoleTypes{Crewmate=0,Impostor=1}
public class AmongUsClient{public static AmongUsClient Instance=new();public bool AmHost=true;}
public class PlayerControl{public byte Id;public List<RoleTypes> Assigned=new();public void RpcSetRole(RoleTypes role)=>Assigned.Add(role);}
public static class MiscUtils{public static Dictionary<byte,PlayerControl> Players=new();public static PlayerControl PlayerById(byte id)=>Players.GetValueOrDefault(id);public static void LogInfo(object level,string text){}}
namespace Events{public static class TownOfUsEventHandlers{public enum LogLevel{Warning}}}
public class DraftSlotState{public byte PlayerId;public int SlotNumber;public ushort ChosenRoleId;public bool HasPicked;public string ForcedRoleName;}
public static class DraftRolePool{public static string GetRoleNameFromId(ushort id)=>id==0?"Crewmate":"Custom";}
public struct Vector3{public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 operator*(Vector3 v,float f)=>new(v.x*f,v.y*f,v.z*f);}
public class Transform{public Vector3 localPosition,localScale=new(1,1,1);}
public class Area{public Transform transform=new();}
public class MeetingHud{public Area[] playerStates;}
public static class Mathf{public static float Min(float a,float b)=>Math.Min(a,b);public static float Abs(float a)=>Math.Abs(a);}
'@
$code+=$ledger+$applier+$layout
$code+=@'
public static class Tests{
 static int n;static void Check(bool b,string s){n++;if(!b)throw new Exception(s);}
 public static int Run(){
  foreach(var threshold in new[]{2,3}){
   var l=new PaperLedger();for(int i=1;i<threshold;i++)Check(!l.Report(i,1,2,i,threshold),"No early curse");
   Check(l.Report(threshold,1,2,threshold,threshold),"Threshold arms curse");Check(l.Due(threshold).Length==0,"Reporting meeting does not kill");Check(l.Due(threshold+1).Single()==((byte)1,(byte)2),"Following meeting is due");Check(!l.Report(threshold,1,2,threshold,threshold),"Duplicate report RPC ignored");l.Cancel(1);Check(l.Due(100).Length==0,"Papetir death cancels curse");
  }
  var ledger=new PaperLedger();ledger.Report(1,1,2,1,2);ledger.Report(2,1,3,2,2);Check(ledger.Due(5).Length==0,"Reports per player are independent");ledger.Report(3,4,2,3,2);Check(ledger.Due(5).Length==0,"Reports per Papetir are independent");Check(!ledger.Report(4,1,1,4,2),"No self-curse exploit");Check(ledger.Credit(1,2)&&!ledger.Credit(1,2)&&ledger.Victims(1)==1,"Unique victims and duplicate death protection");Check(!ledger.Credit(1,1),"Self kill gives no victory progress");ledger.Cancel(1);Check(ledger.Victims(1)==1,"Completed victim credit retained after cancellation");Check(new PaperLedger().Victims(1)==0,"New match state fresh");
  foreach(int count in new[]{16,24}){
   MiscUtils.Players.Clear();var states=new List<DraftSlotState>();for(byte i=0;i<count;i++){MiscUtils.Players[i]=new(){Id=i};states.Add(new(){PlayerId=i,SlotNumber=i+1,HasPicked=true,ChosenRoleId=(ushort)(i==0?0:100+i)});}
   DraftApplier.StorePendingDraftStates(states);states[1].ChosenRoleId=999;Check(DraftApplier.PendingDraftStates[1].ChosenRoleId==101,"Pending assignment is a snapshot, not mutable UI state");DraftApplier.ApplyDraftResults(DraftApplier.PendingDraftStates);
   Check(MiscUtils.Players[0].Assigned.Single()==RoleTypes.Crewmate,"Role id zero is valid Crewmate, never skipped");for(byte i=1;i<count;i++)Check((ushort)MiscUtils.Players[i].Assigned.Single()==100+i,"Every selected role applied at >15 players");
   var areas=Enumerable.Range(0,count).Select(i=>new Area{transform=new(){localPosition=new Vector3(i%3*3,-(i/3)*.65f,0)}}).ToArray();var hud=new MeetingHud{playerStates=areas};ExpandedMeetingLayoutPatch.Postfix(hud);Check(areas.Min(a=>a.transform.localPosition.y)>=-2.601f,"Expanded meeting fits native vertical span");Check(areas.All(a=>a.transform.localScale.x>0&&a.transform.localScale.x<=1),"Only vote cards scaled");Check(areas.Select(a=>(a.transform.localPosition.x,a.transform.localPosition.y)).Distinct().Count()==count,"No overlapping card centres");
  }
  return n;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([Tests]::Run()) production PaperLedger, draft assignment and 16/24-player meeting-layout checks with API doubles."
$rpcs=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Modules/DraftMode/DraftRPCS.cs') -Raw
if($rpcs -notmatch 'turn != DraftManager.CurrentTurnNumber' -or $rpcs -notmatch '!innerNetObject.IsHost\(\)' -or $rpcs -notmatch 'data.TurnNumber < DraftManager.CurrentTurnNumber'){throw 'Stale/unauthorized draft packet protection missing'}
if($rpcs -notmatch 'GetStateForPlayer\(localPlayerId\)\?\.IsPickingNow != true'){throw 'Concurrent picker must not be covered by waiting overlay'}
'PASS draft RPC authority/turn/concurrent-picker guards. Not a live multiplayer or Unity test.'
