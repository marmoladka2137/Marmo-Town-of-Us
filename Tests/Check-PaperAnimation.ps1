param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Modules/Papetir/PaperBodyPaintView.cs') -Raw
$production=$raw.Replace('using Il2CppInterop.Runtime.Attributes;','').Replace('using Reactor.Utilities.Attributes;','').Replace('using UnityEngine;','').Replace('namespace TouMarmo.Modules.Papetir;','namespace TouMarmo.Modules.Papetir {').Replace('[RegisterInIl2Cpp]','').Replace('[HideFromIl2Cpp]','')+'}'
$code=@'
#nullable enable
using System;using System.Reflection;using UnityEngine;using TouMarmo.Modules.Papetir;
namespace UnityEngine {
 public class Obj {public static implicit operator bool(Obj? o)=>o!=null;}
 public class MonoBehaviour:Obj{public MonoBehaviour(IntPtr p){}}
 public class Sprite:Obj{}
 public struct Color{public float r,g,b,a;public Color(float r,float g,float b,float a){this.r=r;this.g=g;this.b=b;this.a=a;}}
 public class SpriteRenderer:Obj{public Sprite? sprite;public bool flipX,flipY,enabled=true,forceRenderingOff;public int sortingLayerID,sortingOrder;public Color color=new(1,1,1,1);}
}
namespace TouMarmo.Modules.Papetir{
 public static class PaperBodyAppearance{
  public static int calls;public static Sprite? unsupported;
  public static System.Collections.Generic.Dictionary<Sprite,Sprite> pixels=new();
  public static Sprite? GetChips(Sprite frame){calls++;if(frame==unsupported)return null;if(!pixels.TryGetValue(frame,out var overlay))pixels[frame]=overlay=new Sprite();return overlay;}
 }
}
'@
$code+=$production
$code+=@'
public static class PaperAnimationTests{
 static int count;static void Check(bool pass,string name){count++;if(!pass)throw new Exception(name);}
 static void Tick(PaperBodyPaintView view)=>typeof(PaperBodyPaintView).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(view,null);
 public static int Run(){
  var a=new Sprite();var b=new Sprite();var source=new SpriteRenderer{sprite=a};var overlay=new SpriteRenderer();
  var view=new PaperBodyPaintView(IntPtr.Zero);view.Configure(source,overlay);
  Check(overlay.enabled&&overlay.sprite==PaperBodyAppearance.pixels[a],"Initial decal matches initial body frame");
  source.sprite=b;Tick(view);
  Check(overlay.enabled&&overlay.sprite==PaperBodyAppearance.pixels[b],"LateUpdate replaces the decal when the falling frame changes");
  Check(overlay.sprite!=PaperBodyAppearance.pixels[a],"No stale frame left visible");
  var calls=PaperBodyAppearance.calls;for(int i=0;i<60;i++)Tick(view);
  Check(PaperBodyAppearance.calls==calls,"Resting body does not rebuild/read pixels every frame");
  source.flipX=true;source.flipY=true;source.sortingLayerID=3;source.sortingOrder=12;source.color=new Color(1,1,1,.4f);Tick(view);
  Check(overlay.flipX&&overlay.flipY,"Mirrored body and decal agree");
  Check(overlay.sortingLayerID==3&&overlay.sortingOrder==13,"Decal stays immediately above its body");
  Check(overlay.color.a==.4f,"Decal follows body fade");
  source.enabled=false;Tick(view);Check(!overlay.enabled,"Disabled body hides decal");
  source.enabled=true;source.forceRenderingOff=true;Tick(view);Check(!overlay.enabled,"Force-hidden body hides decal");
  source.forceRenderingOff=false;source.sprite=null;Tick(view);Check(!overlay.enabled,"Empty animation frame hides decal");
  source.sprite=b;Tick(view);Check(overlay.enabled,"Visible frame restores decal");
  var unsupported=new Sprite();PaperBodyAppearance.unsupported=unsupported;source.sprite=unsupported;Tick(view);
  Check(!overlay.enabled&&overlay.sprite==null,"Unsupported frame cannot retain previous frame's decal");
  source.sprite=a;Tick(view);Check(overlay.enabled&&overlay.sprite==PaperBodyAppearance.pixels[a],"Returning frame reuses matching cached pixels");
  Check(source.sprite==a&&source.color.a==.4f&&source.flipX,"Native frame/colour/orientation untouched");
  return count;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([PaperAnimationTests]::Run()) production falling-frame synchronization checks with Unity doubles."
$appearance=Get-Content (Join-Path $Root 'Modules/Papetir/PaperBodyAppearance.cs') -Raw
if($appearance -notmatch 'SetParent\(source.transform,false\)' -or $appearance -notmatch 'GetComponent<PaperBodyPaintView>' -or $appearance -notmatch 'AddComponent<PaperBodyPaintView>'){throw 'Decal must inherit body transforms and have a single animation follower'}
'PASS parent/body ownership and duplicate-attachment guard. Not a Unity playback or multiplayer test.'
