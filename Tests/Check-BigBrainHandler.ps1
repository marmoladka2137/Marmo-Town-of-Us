param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$runtime=Get-Content (Join-Path $Root 'Modules/BigBrain/BigBrainRuntime.cs') -Raw
$start=$runtime.IndexOf('    public sealed record Attempt(')
$finish=$runtime.IndexOf('[HarmonyPatch(typeof(AssassinModifier)')
$tracking=$runtime.Substring($start,$finish-$start).Trim()
$tracking=$tracking.Substring(0,$tracking.LastIndexOf('}'))
$tracking=$tracking.Replace('[HarmonyPrefix]','').Replace('[HarmonyPostfix]','')
$buttons=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Buttons/TownOfUsButton.cs') -Raw
$class=$buttons.IndexOf('public abstract class TownOfUsTargetButton<T>')
$start=$buttons.IndexOf('    public override void ClickHandler()',$class)
$end=$buttons.IndexOf('    protected override void FixedUpdate(',$start)
$handler=$buttons.Substring($start,$end-$start)
$code=@'
#nullable enable annotations
using System;using System.Collections.Generic;
public class PlayerControl {
 public static PlayerControl LocalPlayer;public Info Data=new();
 public static implicit operator bool(PlayerControl p)=>p!=null;
}
public class Info {public RoleData Role=new();}public class RoleData {public ushort Role=42;}
public struct Color {public static Color clear=>new();public static bool operator ==(Color a,Color b)=>true;public static bool operator !=(Color a,Color b)=>false;public override bool Equals(object o)=>o is Color;public override int GetHashCode()=>0;}
public static class TownOfUsColors {public static bool UseBasic;}
public class TouLocalTabPlayers {public Toggle UseCrewmateTeamColorToggle=new();}public class Toggle {public bool Value;}
public static class LocalSettingsTabSingleton<T> where T:new() {public static T Instance=new();}
public class Image {public Color color;}
public class ButtonView {public Image usesRemainingSprite=new();public void SetUsesRemaining(int count){}}
public interface IKillButton {}
public class CustomActionButton {
 public bool Allowed=true,LimitedUses,HasEffect,EffectActive;public float Timer,Cooldown=20,EffectDuration=4;public int UsesLeft=3,Clicks;
 public Color TextOutlineColor;public ButtonView Button=new();public virtual void ClickHandler(){}
 public bool CanClick()=>Allowed;public virtual void OnClick(){Clicks++;}public void SetTextOutline(Color color){}
}
public class CustomActionButton<T>:CustomActionButton {public T Target;}
'@
$code+="namespace TouMarmo.Modules.BigBrain {public static class BigBrainRuntime {public static List<(PlayerControl Source,PlayerControl Target,ushort Role,bool Failed)> Reports=new();public static void Record(PlayerControl source,PlayerControl target,bool failed,ushort role){Reports.Add((source,target,role,failed));}} public static class BigBrainButtonPatch {"+$tracking+'}}'
$code+='namespace TouMarmo.Modules.Manipulator {public static class ManipulatorButtonGate {public static bool Allowed=true;public static bool Prefix(CustomActionButton b)=>Allowed;}}'
$code+='public class ProductionHandler<T>:CustomActionButton<T> {'+$handler+'}'
$code+=@'
public class KillHandler:ProductionHandler<PlayerControl>,IKillButton {}
public class ThrowHandler:ProductionHandler<PlayerControl> {public override void OnClick(){throw new Exception("intentional");}}
public static class HandlerTests {
 static int checks;static void Check(bool ok,string name){checks++;if(!ok)throw new Exception(name);}
 static void Clear()=>TouMarmo.Modules.BigBrain.BigBrainRuntime.Reports.Clear();
 public static int Run(){
  var source=new PlayerControl();var target=new PlayerControl();PlayerControl.LocalPlayer=source;
  var b=new ProductionHandler<PlayerControl>{Target=target};Clear();b.ClickHandler();
  var r=TouMarmo.Modules.BigBrain.BigBrainRuntime.Reports;
  Check(b.Clicks==1&&b.Timer==20,"Original ability/cooldown runs");Check(r.Count==1&&!r[0].Failed,"Successful targeted click reported once");
  Check(r[0].Source==source&&r[0].Target==target&&r[0].Role==42,"Source, target and role retained");
  b=new(){Target=target,LimitedUses=true};Clear();b.ClickHandler();Check(b.UsesLeft==2&&r.Count==1&&!r[0].Failed,"Limited uses tracked without changing consumption");
  b=new(){Target=target,HasEffect=true};Clear();b.ClickHandler();Check(b.EffectActive&&b.Timer==4&&r.Count==1&&!r[0].Failed,"Timed effect remains active and detected");
  b=new(){Target=target,Allowed=false};Clear();b.ClickHandler();Check(b.Clicks==0&&r.Count==0&&b.Timer==0,"Unavailable button neither activates nor reports");
  b=new(){Target=null};Clear();b.ClickHandler();Check(r.Count==0,"Missing target not reported");
  PlayerControl.LocalPlayer=null;b=new(){Target=target};Clear();b.ClickHandler();Check(r.Count==0,"No local player is safe");PlayerControl.LocalPlayer=source;
  var k=new KillHandler{Target=target};Clear();k.ClickHandler();Check(k.Clicks==1&&r.Count==0,"Kill remains handled by existing murder pipeline");
  var other=new ProductionHandler<object>{Target=new object()};Clear();other.ClickHandler();Check(other.Clicks==1&&r.Count==0,"Non-player targets remain unaffected");
  var broken=new ThrowHandler{Target=target};Clear();try{broken.ClickHandler();throw new Exception("Exception swallowed");}catch(Exception ex){Check(ex.Message=="intentional","Ability exceptions retain normal behavior");}
  Check(r.Count==0,"Interrupted ability produces no false successful report");
  b=new(){Target=target,Timer=20};Clear();b.ClickHandler();Check(r.Count==1&&r[0].Failed,"Unchanged state keeps previous failed-attempt classification");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([HandlerTests]::Run()) production Big Brain and generic click-handler assertions with API doubles."
if($runtime.Contains('AccessTools.DeclaredMethod(typeof(TownOfUsTargetButton<PlayerControl>)')){throw 'Unsafe generic detour reintroduced'}
'PASS: no explicit closed-generic Big Brain patch target.'
