param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$text=Get-Content (Join-Path $Root 'Modules/Plumber/PlumberNailRuntime.cs') -Raw
$method=[regex]::Match($text,'public static bool InRange\(.*?false\);',[Text.RegularExpressions.RegexOptions]::Singleline).Value
if(!$method){throw 'Missing range predicate'}
$code=@'
using System;
public class Obj {public static implicit operator bool(Obj o)=>o!=null;}
public struct Vector2 {public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}public static float Distance(Vector2 a,Vector2 b)=>(float)Math.Sqrt((a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y));}
public class Transform {public Vector2 position;}
public class Bounds {public Vector2 center;}
public class Collider {public Bounds bounds=new();}
public class PlayerControl:Obj {public Collider Collider=new();public Vector2 Position;public Vector2 GetTruePosition()=>Position;}
public class Vent:Obj {public Transform transform=new();public float UsableDistance=.8f;}
public static class Constants {public const int ShipOnlyMask=512,ShipAndAllObjectsMask=6656;}
public static class PhysicsHelpers {public static bool Wall;public static int Mask;public static bool AnythingBetween(Collider c,Vector2 a,Vector2 b,int mask,bool trigger){Mask=mask;return Wall||mask!=Constants.ShipOnlyMask;}}
'@
$code+='public static class ProductionRange {'+$method+'}'
$code+=@'
public static class RangeTests {
 public static int Run(){int n=0;var p=new PlayerControl();var vent=new Vent();
  vent.transform.position=new Vector2(.5f,0);
  if(!ProductionRange.InRange(p,vent))throw new Exception("Vent/prop must not block its own interaction");n++;
  if(PhysicsHelpers.Mask!=Constants.ShipOnlyMask)throw new Exception("Use vanilla vent wall mask");n++;
  PhysicsHelpers.Wall=true;if(ProductionRange.InRange(p,vent))throw new Exception("Ship wall must block nail placement");n++;
  PhysicsHelpers.Wall=false;vent.transform.position=new Vector2(2,0);
  if(ProductionRange.InRange(p,vent))throw new Exception("Distant vent must be rejected");n++;
  if(ProductionRange.InRange(null,vent)||ProductionRange.InRange(p,null))throw new Exception("Missing player or vent");n++;
  return n;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([RangeTests]::Run()) production Plumber range checks with API doubles; not native map geometry."
