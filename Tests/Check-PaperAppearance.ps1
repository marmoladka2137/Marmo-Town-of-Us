param([string]$Root=(Join-Path $PSScriptRoot '..'),[string]$NativeInput='', [string]$OverlayOutput='')
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Modules/Papetir/PaperPaintPixels.cs') -Raw
$production=$raw.Replace('using UnityEngine;','').Replace('namespace TouMarmo.Modules.Papetir;','namespace TouMarmo.Modules.Papetir {')+'}'
$code=@'
using System;using System.Linq;using UnityEngine;using TouMarmo.Modules.Papetir;
namespace UnityEngine {public struct Color32 {public byte r,g,b,a;public Color32(byte r,byte g,byte b,byte a){this.r=r;this.g=g;this.b=b;this.a=a;}}}
'@
$code+=$production
$code+=@'
public static class PaperAppearanceTests {
 static int count;static void Check(bool pass,string name){count++;if(!pass)throw new Exception(name);}
 public static int Run(){
  Check(PaperPaintPixels.Create(new Color32[2],2,2).Length==0,"Malformed input rejected");
  Check(PaperPaintPixels.Create(Array.Empty<Color32>(),0,4).Length==0,"Zero-size input rejected");
  Check(PaperPaintPixels.Create(new Color32[400],20,20).All(p=>p.a==0),"Empty corpse stays invisible");
  const int w=100,h=80;var source=new Color32[w*h];
  for(int y=10;y<70;y++)for(int x=10;x<90;x++)source[y*w+x]=new Color32(180,20,20,255);
  for(int y=10;y<70;y++){source[y*w+10]=new Color32(0,0,0,255);source[y*w+89]=new Color32(0,0,0,255);}
  var saved=source.ToArray();var output=PaperPaintPixels.Create(source,w,h);
  Check(source.SequenceEqual(saved),"Native corpse pixels unchanged");
  var painted=output.Select((p,i)=>(p,i)).Where(v=>v.p.a>0).ToArray();
  Check(painted.Length>25&&painted.Length<source.Count(p=>p.a>0)*.08,"Visible small chips; no whole-silhouette fill");
  Check(painted.All(v=>source[v.i].a>=220),"Paint never extends outside the original alpha");
  Check(painted.All(v=>Math.Max(source[v.i].r,Math.Max(source[v.i].g,source[v.i].b))>=55),"Black outline preserved");
  Check(painted.Any(v=>v.i%w<w/2)&&painted.Any(v=>v.i%w>w/2),"Both lower edges have visible chips");
  Check(painted.All(v=>v.i/w<h*.65),"Top and bone untouched");
  Check(painted.Any(v=>v.i/w>=h/2),"Third small chip above the original two");
  Check(output.SequenceEqual(PaperPaintPixels.Create(source,w,h)),"Identical result on each client");
  return count;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([PaperAppearanceTests]::Run()) production paint-chip geometry/alpha/source-preservation checks."
if($NativeInput){
 $inputData=Get-Content -LiteralPath $NativeInput -Raw | ConvertFrom-Json
 $pixels=[UnityEngine.Color32[]]@($inputData.pixels | ForEach-Object {[UnityEngine.Color32]::new($_[0],$_[1],$_[2],$_[3])})
 $overlay=[TouMarmo.Modules.Papetir.PaperPaintPixels]::Create($pixels,$inputData.width,$inputData.height)
 $painted=0;$opaque=0
 for($i=0;$i -lt $overlay.Length;$i++){
  if($pixels[$i].a -gt 0){$opaque++}
  if($overlay[$i].a -gt 0){$painted++;if($pixels[$i].a -lt 220){throw 'Native sprite alpha exceeded'}}
 }
 if($painted -le 0 -or $painted -ge $opaque*.08){throw "Unexpected native sprite chip coverage $painted/$opaque"}
 "PASS native game sprite: $painted painted pixels / $opaque opaque pixels; no alpha spill."
 if($OverlayOutput){[byte[]]$rgba=@($overlay|ForEach-Object {$_.r;$_.g;$_.b;$_.a});[IO.File]::WriteAllBytes($OverlayOutput,$rgba)}
}
$appearance=Get-Content (Join-Path $Root 'Modules/Papetir/PaperBodyAppearance.cs') -Raw
if($appearance -match 'AddComponent<SpriteMask>|VisibleInsideMask|mask\.sprite'){throw 'Whole-corpse stencil mask must not be created'}
'PASS no whole-corpse SpriteMask. Unity rendering and multiplayer not tested.'
