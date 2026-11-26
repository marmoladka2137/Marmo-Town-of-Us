param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Modules/Gravedigger/GravediggerRoleSetup.cs') -Raw
$start=$raw.IndexOf('    public static HashSet<object>? CaptureLocal')
$end=$raw.IndexOf('    public static bool CanAssign', $start)
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;
public class Obj { public static implicit operator bool(Obj? o)=>o!=null; }
public class Role:Obj {public int Id;}
public class Data:Obj {public Role Role=new(){Id=1};}
public class PlayerControl:Obj {public bool AmOwner=true;public Data Data=new();}
public class GameObject {public bool active;public void SetActive(bool v)=>active=v;}
public class ActionButton:Obj {public GameObject gameObject=new();}
public class Ability {public int RoleId,MaxUses=3,UsesLeft,UsesSoFar=4,ResetCount,Updates;public float InitialCooldown=7,Timer=99;public bool TimerPaused=true,Effect=true;public ActionButton ButtonObject=new();public ActionButton Button=>ButtonObject;
 public bool Enabled(Role r)=>RoleId==0||RoleId==r.Id;public void SetUses(int n)=>UsesLeft=Math.Max(0,n);public void ResetCooldownAndOrEffect(){ResetCount++;Effect=false;Timer=42;}public void UpdateHandler(MeetingHud hud)=>Updates++;}
public static class CustomButtonManager {public static List<Ability> Buttons=new();}
public static class MeetingButtonManager {public static List<Ability> UntargetedButtons=new(),TargetedButtons=new();}
public class MeetingHud:Obj {public static MeetingHud? Instance;}
public class ExileController:Obj {public static ExileController? Instance;}
public class HudManager:Obj {public static HudManager? Instance=new();public bool Visible;public Role? Role;public void SetHudActive(PlayerControl p,Role r,bool visible){Role=r;Visible=visible;}}
public static class Setup {
'@
$code+=$raw.Substring($start,$end-$start)+'}'
$code+=@'
public static class Tests {
 static int n;static void Check(bool b,string s){n++;if(!b)throw new Exception(s);}
 public static int Run(){
 var p=new PlayerControl();var old=new Ability{RoleId=1};var fresh=new Ability{RoleId=2};var modifier=new Ability{RoleId=0,UsesLeft=1};var disabled=new Ability{RoleId=3};
 var oldMeeting=new Ability{RoleId=1};oldMeeting.Button.gameObject.active=true;
 var newMeeting=new Ability{RoleId=2};var meetingMod=new Ability{RoleId=0,UsesLeft=1};var target=new Ability{RoleId=2,MaxUses=2};
 CustomButtonManager.Buttons.AddRange(new[]{old,fresh,modifier,disabled});MeetingButtonManager.UntargetedButtons.AddRange(new[]{oldMeeting,newMeeting,meetingMod});MeetingButtonManager.TargetedButtons.Add(target);
 var before=Setup.CaptureLocal(p);Check(before!=null&&before.Contains(old)&&before.Contains(modifier)&&before.Contains(oldMeeting),"Capture existing abilities before role change");Check(!before!.Contains(fresh),"New role not in prior abilities");
 p.Data.Role=new(){Id=2};MeetingHud.Instance=new();Setup.ActivateLocal(p,before);
 Check(fresh.UsesLeft==3,"New role limited uses initialized");Check(fresh.Timer==7,"Initial cooldown preserved");Check(!fresh.TimerPaused&&!fresh.Effect&&fresh.ResetCount==1,"Stale pause/effect cleared");
 Check(modifier.UsesLeft==1&&modifier.ResetCount==0&&modifier.Timer==99,"Existing modifier state untouched");Check(old.ResetCount==0&&disabled.ResetCount==0,"Other role buttons untouched");
 Check(!oldMeeting.Button.gameObject.active,"Old inheritance meeting button hidden");Check(newMeeting.Button.gameObject.active,"Inherited meeting ability visible immediately");Check(newMeeting.UsesLeft==3&&newMeeting.UsesSoFar==0&&!newMeeting.TimerPaused,"Meeting ability initialized");Check(newMeeting.Timer==7&&newMeeting.Updates==1,"Meeting button refresh");Check(meetingMod.UsesLeft==1&&meetingMod.Timer==99,"Meeting modifier state preserved");Check(target.UsesLeft==2&&target.Timer==7,"Targeted meeting ability initialized");
 Check(!HudManager.Instance!.Visible&&HudManager.Instance.Role==p.Data.Role,"Round HUD remains hidden during meeting");
 var remote=new PlayerControl{AmOwner=false};Check(Setup.CaptureLocal(remote)==null,"Remote player does not snapshot local buttons");Setup.ActivateLocal(remote,null);Check(fresh.ResetCount==1,"Remote inheritance cannot reset local HUD");
 MeetingHud.Instance=null;Setup.ActivateLocal(p,before);Check(HudManager.Instance.Visible,"Round HUD available outside meeting");
 ExileController.Instance=new();Setup.ActivateLocal(p,before);Check(!HudManager.Instance.Visible,"Round HUD hidden during exile");ExileController.Instance=null;
 fresh.MaxUses=0;Setup.ActivateLocal(p,before);Check(fresh.UsesLeft==0,"Unlimited ability keeps zero convention");return n;
 }}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
$count=[Tests]::Run()
$runtime=Get-Content (Join-Path $Root 'Modules/Gravedigger/GravediggerRuntime.cs') -Raw
if($runtime.IndexOf('var previousButtons = GravediggerRoleSetup.CaptureLocal(owner);') -gt $runtime.IndexOf('owner.ChangeRole(roleId);')){throw 'Snapshot must precede role change'}
if($runtime -notmatch 'ActivateLocal\(owner, previousButtons\)' -or $runtime -match 'ButtonResetPatches.ResetCooldowns'){throw 'Must use scoped role activation'}
$menu=Get-Content (Join-Path $Root 'Modules/Gravedigger/GravediggerRoleMenu.cs') -Raw
if($menu -notmatch 'MiscUtils.GetRoleTmpIcon\(entry.Role\)' -or $menu -match 'panel.SetRole\('){throw 'Role cards must use inline meeting icons'}
if($menu -notmatch 'PlayerIcon.gameObject.SetActive\(false\)' -or $menu -notmatch 'Label\('){throw 'Role card body must be hidden'}
$button=Get-Content (Join-Path $Root 'MeetingAbilities/GravediggerInheritButton.cs') -Raw
if($button -notmatch 'GravediggerAssets.Inherit' -or $button -match 'RememberButtonSprite'){throw 'Inherit must use its own embedded icon'}
"PASS: $count assertions against production CaptureLocal/ActivateLocal; RPC order and inline role-card/own icon wiring verified. Unity visuals and multiplayer not executed."

