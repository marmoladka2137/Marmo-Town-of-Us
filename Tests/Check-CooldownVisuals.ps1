param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Patches/CooldownVisualStyle.cs') -Raw
$production=$raw.Replace('using TMPro;','').Replace('using UnityEngine;','').Replace('namespace TouMarmo.Patches;','namespace TouMarmo.Patches {')+'}'
$code=@'
#nullable enable
using System;using TMPro;using UnityEngine;using TouMarmo.Patches;
namespace UnityEngine {
 public class Obj {public static implicit operator bool(Obj? o)=>o!=null;}
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 one=>new(1,1,1);}
 public class Transform:Obj {public Transform? parent;public Vector3 lossyScale=Vector3.one,localScale=Vector3.one;}
 public static class Mathf {public static float Abs(float f)=>Math.Abs(f);}
}
namespace TMPro {
 public class Font:Obj{}
 public enum FontStyles{Normal,Bold} public enum FontWeight{Regular,Bold} public enum TextOverflowModes{Truncate,Overflow}
 public class TextMeshPro:Obj {public bool enableAutoSizing=true,enableWordWrapping=true,Active=true;public float fontSize=9,fontSizeMin=1,fontSizeMax=12;public Font font=new();public FontStyles fontStyle;public FontWeight fontWeight;public TextOverflowModes overflowMode;public Transform transform=new();public string text="8.4",Color="green";}
}
'@
$code+=$production
$code+=@'
public static class Tests {
 static int n;static void Check(bool ok,string name){n++;if(!ok)throw new Exception(name);}
 public static int Run(){
  var sample=new TextMeshPro{fontSize=3,fontStyle=FontStyles.Bold,fontWeight=FontWeight.Bold};
  var a=new TextMeshPro();a.transform.parent=new Transform{lossyScale=new Vector3(.5f,.5f,1)};
  var b=new TextMeshPro{fontSize=20,text="8.0"};b.transform.parent=new Transform{lossyScale=new Vector3(2,2,1)};
  CooldownVisualStyle.Apply(a,sample,3,new Vector3(1,1,1));CooldownVisualStyle.Apply(b,sample,3,new Vector3(1,1,1));
  Check(a.fontSize==b.fontSize&&a.fontSize==3,"Different cooldowns share point size");Check(!a.enableAutoSizing&&!b.enableAutoSizing,"Autosizing cannot resize digits independently");
  Check(a.fontSizeMin==3&&a.fontSizeMax==3&&b.fontSizeMin==3&&b.fontSizeMax==3,"Fixed size limits");
  Check(a.transform.localScale.x==2&&b.transform.localScale.x==.5f,"Parent scale compensated");Check(a.transform.localScale.x*.5f==b.transform.localScale.x*2,"Same rendered world scale");
  Check(a.font==sample.font&&b.font==sample.font,"Same font asset");Check(a.fontStyle==sample.fontStyle&&a.fontWeight==sample.fontWeight,"Same glyph style/weight");
  Check(a.text=="8.4"&&b.text=="8.0","Cooldown text values unchanged");Check(a.Active&&b.Active&&a.Color=="green","Visibility and effect colours unchanged");
  Check(!a.enableWordWrapping&&a.overflowMode==TextOverflowModes.Overflow,"Decimals remain on one line without clipping");
  for(int i=0;i<30;i++)CooldownVisualStyle.Apply(a,sample,3,Vector3.one);Check(a.transform.localScale.x==2,"Repeated frames do not compound scale");
  a.transform.parent.lossyScale=new Vector3(0,0,0);CooldownVisualStyle.Apply(a,sample,3,Vector3.one);Check(float.IsFinite(a.transform.localScale.x),"Zero-scale animation cannot create infinity");
  var missing=new TextMeshPro();CooldownVisualStyle.Apply(missing,sample,float.NaN,Vector3.one);Check(missing.fontSize==9,"Invalid reference size ignored");
  CooldownVisualStyle.Apply(null,sample,3,Vector3.one);Check(true,"Missing cooldown text safe");
  var root=new TextMeshPro();CooldownVisualStyle.Apply(root,sample,3,new Vector3(2,3,1));Check(root.transform.localScale.x==2&&root.transform.localScale.y==3,"Text without parent handled");
  return n;
 }
}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
"PASS $([Tests]::Run()) production cooldown typography/parent-scale assertions with Unity/TMP doubles."
if($raw -match '\.text\s*=|\.SetActive\(|\b(?:Timer|Cooldown|UsesLeft|EffectDuration)\s*='){throw 'Style helper changes gameplay/text/visibility'}
foreach($f in @('Buttons/FurasButtons.cs','Buttons/NecromancerButtons.cs')){
 $text=Get-Content (Join-Path $Root $f) -Raw
 if($text -match 'timer.fontSize\s*=|timer.transform.localScale\s*='){throw 'Role-specific cooldown sizing overrides remain'}
}
'PASS one shared cooldown style; values, visibility and per-role sizing overrides untouched/removed as intended. No game launched.'
$reference=Get-Content (Join-Path $Root 'Patches/AbilityVisualSizes.cs') -Raw
if($reference -match 'OfType<EngineerFixButton>' -or $reference -notmatch 'var reference=hud.AbilityButton;' -or $reference -notmatch 'cooldownFontSize=sample.fontSize;'){throw 'Use the untouched native template that MiraAPI clones, not the already rendered Engineer button'}
if($reference -notmatch 'nameof\(HudManager.Start\)' -or $reference -notmatch 'HarmonyPriority\(Priority.First\)' -or $reference -notmatch 'typographyReference != reference'){throw 'Capture native typography at HUD creation before custom buttons render; retain fallback for a new HUD'}
'PASS original MiraAPI ability template captured before custom-button rendering; fixed size retained across frames.'
