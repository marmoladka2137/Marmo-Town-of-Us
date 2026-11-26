param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Modules/Exorcist/ExorcistSuspectRules.cs') -Raw
$part=$part.Replace('namespace TouMarmo.Modules.Exorcist;', 'namespace TouMarmo.Modules.Exorcist {')
$code='using System; using System.Linq; using System.Collections.Generic;' + $part + '}'
$code+=@'
public static class SuspectTests {
 static int count;
 static void Check(bool value){if(!value)throw new Exception("Invalid suspect pair");count++;}
 public static int Run(){
  for(int mask=0;mask<64;mask++){
   var live=Enumerable.Range(0,6).Where(i=>(mask&(1<<i))!=0).Select(i=>(byte)i).ToArray();
   for(byte killer=0;killer<6;killer++)for(byte exorcist=0;exorcist<6;exorcist++){
    var decoys=TouMarmo.Modules.Exorcist.ExorcistSuspectRules.Decoys(killer,exorcist,live);
    Check(decoys.Distinct().Count()==decoys.Length);
    Check(decoys.All(id=>live.Contains(id)&&id!=killer&&id!=exorcist));
    for(int choice=-1;choice<=decoys.Length;choice++)foreach(bool first in new[]{true,false}){
     var pair=TouMarmo.Modules.Exorcist.ExorcistSuspectRules.Select(killer,exorcist,live,choice,first);
     bool valid=killer!=exorcist&&live.Contains(killer)&&choice>=0&&choice<decoys.Length;
     Check(pair.Length==(valid?2:0));
     if(!valid)continue;
     Check(pair[0]!=pair[1]&&pair.Contains(killer)&&pair.All(live.Contains)&&!pair.Contains(exorcist));
     Check(pair[first?0:1]==killer&&pair[first?1:0]==decoys[choice]);
    }
   }
  }
  var duplicate=TouMarmo.Modules.Exorcist.ExorcistSuspectRules.Decoys(1,2,new byte[]{1,2,3,3,4});
  Check(duplicate.SequenceEqual(new byte[]{3,4}));
  return count;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([SuspectTests]::Run()) suspect selection assertions (living players, distinctness, real killer, order, absent candidates)."
