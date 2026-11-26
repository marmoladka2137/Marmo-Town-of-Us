param([string]$Root=(Join-Path $PSScriptRoot ".."))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Modules/Cosmetics/HeadBackground.cs') -Raw
$code='using System;using System.Collections.Generic;'+$part.Replace('namespace TouMarmo.Modules.Cosmetics;','namespace TouMarmo.Modules.Cosmetics {')+'}'
$code+=@'
public static class MaskTests {
 public static void Run(){
  var mask=TouMarmo.Modules.Cosmetics.HeadBackground.EdgeMask(5,5,i=>i%5==0||i%5==4||i<5||i>=20||i==12);
  for(int i=0;i<25;i++)if(mask[i]!=(i%5==0||i%5==4||i<5||i>=20))throw new Exception("Internal black region must remain");
  var full=TouMarmo.Modules.Cosmetics.HeadBackground.EdgeMask(3,2,i=>true);foreach(var b in full)if(!b)throw new Exception("Connected background");
  var none=TouMarmo.Modules.Cosmetics.HeadBackground.EdgeMask(3,2,i=>false);foreach(var b in none)if(b)throw new Exception("Opaque subject");
 }
}
'@
Add-Type -TypeDefinition $code
[MaskTests]::Run()
'PASS: edge flood fill and preserved isolated dark subject regions.'
