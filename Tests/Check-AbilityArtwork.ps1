param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Patches/AbilityVisualSizes.cs') -Raw
$start=$raw.IndexOf('    public const float MiraAbilityDiameter')
$end=$raw.IndexOf('    public static bool IsPracticeTool', $start)
$menu=Get-Content (Join-Path $Root 'Modules/Gravedigger/GravediggerRoleMenu.cs') -Raw
$label=$menu.Substring($menu.IndexOf('    internal static string Label'))
$code=@'
using System;using System.Linq;using System.Collections.Generic;
public class Obj{public static implicit operator bool(Obj o)=>o!=null;}
public struct Color32{public byte a;}
public struct Vector3{public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 one=>new Vector3(1,1,1);public static Vector3 operator*(Vector3 v,float n)=>new Vector3(v.x*n,v.y*n,v.z*n);}
public class Transform:Obj{public Transform parent;public Vector3 localScale=new Vector3(5,6,7),lossyScale=Vector3.one;}
public enum SpriteDrawMode{Simple,Sliced}
public class Bounds{public Vector3 size=new Vector3(4,4,0);}
public class Texture:Obj{public string name="TouMarmo.Resources.Test.png";public int width=100,height=100,Reads;public bool isReadable=true;public Color32[] Pixels=new Color32[10000];public Color32[] GetPixels32(){Reads++;return Pixels;}}
public class Sprite:Obj{public Texture texture=new Texture();public Bounds bounds=new Bounds();public float pixelsPerUnit=100;public int Id;public int GetInstanceID()=>Id;}
public class SpriteRenderer:Obj{public Sprite sprite=new Sprite();public Transform transform=new Transform();public SpriteDrawMode drawMode=SpriteDrawMode.Sliced;}
public static class Mathf{public static float Max(float a,float b)=>Math.Max(a,b);public static float Abs(float n)=>Math.Abs(n);}
public static class Visual {
'@
$code+=$raw.Substring($start,$end-$start)+'} public static class Cards {'+$label
$code+=@'
public static class Tests{
static int n;static void Check(bool b,string s){n++;if(!b)throw new Exception(s);}
public static int Run(){
 var pixels=new Color32[10000];for(int y=30;y<70;y++)for(int x=40;x<60;x++)pixels[y*100+x].a=255;
 Check(Visual.VisiblePixelExtent(pixels,100,100)==40,"Visible dimensions exclude transparent padding");
 Check(Visual.VisiblePixelExtent(new Color32[12],4,3)==0,"Empty texture safe");Check(Visual.VisiblePixelExtent(pixels,0,100)==0,"Invalid size safe");Check(Visual.VisiblePixelExtent(pixels,99,100)==0,"Mismatched pixels safe");
 pixels[0].a=32;Check(Visual.VisiblePixelExtent(pixels,100,100)==40,"Near transparent edge noise ignored");pixels[0].a=0;
 var g=new SpriteRenderer();g.sprite.texture.Pixels=pixels;Visual.FitGraphic(g);
 Check(Math.Abs(g.transform.localScale.x*0.4f-1.10f)<.0001f,"Visible MTOU artwork uses native Mira fallback size");Check(g.transform.localScale.x==g.transform.localScale.y,"Uniform scale preserves proportions");Check(g.drawMode==SpriteDrawMode.Simple,"No sliced sprite distortion");Check(g.sprite.texture.Reads==1,"Alpha measured once");Visual.FitGraphic(g);Check(g.sprite.texture.Reads==1,"Cached measurement avoids per frame pixel copies");
 var pollinate=new SpriteRenderer();pollinate.sprite.Id=3;pollinate.sprite.texture.name="TouMarmo.Resources.Icons.SweetsPollinate.png";pollinate.sprite.texture.Pixels=pixels;Visual.FitGraphic(pollinate);Check(Math.Abs(pollinate.transform.localScale.x*.4f-1.10f)<.0001f,"Pollinate matches all other ability icons");
 var bury=new SpriteRenderer();bury.sprite.Id=4;bury.sprite.texture.name="TouMarmo.Resources.Gravedigger.Bury.png";bury.sprite.texture.Pixels=pixels;Visual.FitGraphic(bury);Check(Math.Abs(bury.transform.localScale.x*.4f-1.10f)<.0001f,"Bury matches all other ability icons");
 var mira=new SpriteRenderer();mira.sprite.Id=1;mira.sprite.texture.name="MiraAtlas";Visual.FitGraphic(mira);Check(Math.Abs(mira.transform.localScale.x*4-1.10f)<.0001f,"Atlas-backed Mira icon matches the common size");Check(mira.sprite.texture.Reads==0,"Native atlas not read as though it were one icon");
 var unreadable=new SpriteRenderer();unreadable.sprite.Id=2;unreadable.sprite.texture.isReadable=false;Visual.FitGraphic(unreadable);Check(unreadable.sprite.texture.Reads==0,"Unreadable texture never read");Check(Math.Abs(unreadable.transform.localScale.x-.275f)<.0001f,"Unreadable texture safely uses sprite bounds");
 g.transform.parent=new Transform{lossyScale=new Vector3(.5f,2,1)};Visual.FitGraphic(g,1.32f);
 Check(Math.Abs(g.transform.localScale.x*.5f*.4f-1.32f)<.0001f&&Math.Abs(g.transform.localScale.y*2*.4f-1.32f)<.0001f,"Different parents match the live reference in world space");
 var saved=g.transform.localScale;g.transform.parent.lossyScale=new Vector3(0,0,0);Visual.FitGraphic(g,1.32f);Check(g.transform.localScale.x==saved.x&&g.transform.localScale.y==saved.y,"Hidden zero-scale parent cannot produce infinity");
 g.transform.parent=null;Visual.FitGraphic(g,float.NaN);Check(g.transform.localScale.x==saved.x,"Invalid reference does not corrupt scale");
 Visual.FitGraphic(null);Check(true,"Null renderer safe");
 var card=Cards.Label("<sprite name=\"Medic\">","Medic","Test<color=red>");Check(card.StartsWith("<sprite name=\"Medic\">Medic\n"),"Inline role icon immediately precedes role name");Check(card.Contains("<color=white>Test&lt;color=red&gt;"),"Player label safely formatted");Check(!card.Contains("Protective"),"No unwanted category line");Check(card.Split('\n').Length==2,"Exactly two nameplate lines");return n;
}}
'@
$code='#nullable disable'+"`n"+$code.Replace('SpriteRenderer? graphic','SpriteRenderer graphic')
Add-Type -TypeDefinition $code
"PASS $([Tests]::Run()) production alpha-fitting and role-nameplate assertions; not a Unity render."
if($raw -match 'b.transform.localScale\s*=|Sprite\.Create'){throw 'Native root resize/sprite recreation reintroduced'}
if($raw -match 'PollinateDiameter|\? 1\.10f|pollinate \? \.88f'){throw 'Per-role icon/label exceptions must be removed'}
if($raw -notmatch 'b.graphic!=report' -or $raw -notmatch 'report.sprite.bounds.size.x'){throw 'Live Report reference must stay unmodified'}

