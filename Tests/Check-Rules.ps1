param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$source='using System; using System.Linq; using System.Collections.Generic; using System.Numerics;' + [Environment]::NewLine
foreach($name in @('NewRoleRules','AuditRules','BodyGroupRules','Simp/SimpRules','Necromancer/CorpsePathfinder')) {
 $part=Get-Content -Raw (Join-Path $Root "Modules/$name.cs")
 $part=$part.Replace('using System.Numerics;','')
 $part=[regex]::Replace($part,'namespace TouMarmo.Modules(?:\.Simp|\.Necromancer)?;', 'namespace TouMarmo.Modules {')
 $source += $part + '}' + [Environment]::NewLine
}
$source+=@"
public static class MarmoRuleTests {
 static int checks;
 static void Check(bool value,string name){checks++;if(!value)throw new Exception(name);}
 public static int Run(){
  Check(TouMarmo.Modules.NewRoleRules.ExecutionerChoiceOpen(false,false),"Executioner opens in initial round");
  Check(!TouMarmo.Modules.NewRoleRules.ExecutionerChoiceOpen(false,true),"Executioner cannot use both choices");
  Check(!TouMarmo.Modules.NewRoleRules.ExecutionerChoiceOpen(true,false),"Executioner closes at first meeting");
  Check(!TouMarmo.Modules.NewRoleRules.ExecutionerChoiceOpen(true,true),"Executioner stays closed after meeting");
  Check(!TouMarmo.Modules.NewRoleRules.DoctorReady(0,0,50),"No eligible crew");
  for(int living=1;living<=14;living++) for(int percent=25;percent<=100;percent+=5){
   var required=(int)Math.Ceiling(living*percent/100.0);
   Check(TouMarmo.Modules.NewRoleRules.DoctorReady(living,required,percent),"Doctor threshold");
   Check(!TouMarmo.Modules.NewRoleRules.DoctorReady(living,required-1,percent),"Doctor rounds up");
  }
  Check(TouMarmo.Modules.NewRoleRules.DoctorReady(2,1,50),"Death changes denominator");
  Check(!TouMarmo.Modules.NewRoleRules.DoctorReady(3,1,50),"Living crew included");
  Check(TouMarmo.Modules.NewRoleRules.CanSteal(0,3,false,false),"Initial steal");
  Check(!TouMarmo.Modules.NewRoleRules.CanSteal(1,3,true,false),"Once per round");
  Check(!TouMarmo.Modules.NewRoleRules.CanSteal(3,3,false,false),"Lifetime limit");
  Check(!TouMarmo.Modules.NewRoleRules.CanSteal(1,3,false,true),"No overlapping loan");
  Check(TouMarmo.Modules.NewRoleRules.CanSteal(1,3,false,false),"Next round");
  int mistakes=0; int survived=0;
  for(int guess=0;guess<3;guess++){if(!TouMarmo.Modules.NewRoleRules.TripleShotUsed(mistakes)){mistakes++;survived++;}}
  Check(survived==2 && TouMarmo.Modules.NewRoleRules.TripleShotUsed(mistakes),"Two protected mistakes then death");
  Check(30*TouMarmo.Modules.SimpRules.Multiplier(true,false,1.25f)==22.5f,"Simp reduction");
  Check(30*TouMarmo.Modules.SimpRules.Multiplier(true,true,1.25f)==37.5f,"Simp penalty");
  Check(TouMarmo.Modules.SimpRules.Multiplier(false,false,1.25f)==1,"No target");
  Func<Vector2,Vector2,bool> wall=(a,b)=>{
   for(int i=0;i<=100;i++){var p=Vector2.Lerp(a,b,i/100f);if(p.X>0.8f&&p.X<1.2f&&p.Y>-1&&p.Y<1)return false;}return true;};
  var start=Vector2.Zero;var goal=new Vector2(2,0);
  var path=TouMarmo.Modules.CorpsePathfinder.Find(start,goal,wall);
  Check(path.Count>0 && path.Last()==goal,"Detour reaches killer");
  var previous=start;foreach(var point in path){Check(wall(previous,point),"Collision-free path");previous=point;}
  Check(TouMarmo.Modules.CorpsePathfinder.Find(start,goal,(a,b)=>false).Count==0,"Unreachable stops");
  Check(TouMarmo.Modules.CorpsePathfinder.Find(start,goal,wall,0).Count==0,"Budget stops safely");
  var moved=new Vector2(3,2);Check(TouMarmo.Modules.CorpsePathfinder.Find(start,moved,wall).Last()==moved,"Moving destination recalculated");
  for(int mask=0;mask<128;mask++) {
   bool alive=(mask&1)!=0,connected=(mask&2)!=0,crew=(mask&4)!=0,self=(mask&8)!=0,current=(mask&16)!=0,reserved=(mask&32)!=0,spectator=(mask&64)!=0;
   Check(TouMarmo.Modules.AuditRules.ExecutionerCandidate(alive,connected,crew,self,current,reserved,spectator)==(mask==7),"Executioner eligibility matrix");
  }
  var candidates=new[]{(id:1,crew:true,alive:true),(id:2,crew:false,alive:true),(id:3,crew:true,alive:false),(id:4,crew:true,alive:true)};
  var eligible=candidates.Where(c=>TouMarmo.Modules.AuditRules.ExecutionerCandidate(c.alive,true,c.crew,false,c.id==1,false,false)).ToArray();
  Check(eligible.Length==1 && eligible[0].id==4,"Only alternative living Crewmate selected");
  Check(!candidates.Take(1).Any(c=>TouMarmo.Modules.AuditRules.ExecutionerCandidate(c.alive,true,c.crew,false,true,false,false)),"No candidate does not consume a choice");
  for(int mask=0;mask<16;mask++) {
   bool cancelled=(mask&1)!=0,kill=(mask&2)!=0,dirtek=(mask&4)!=0,bond=(mask&8)!=0;
   Check(TouMarmo.Modules.AuditRules.ProtectionCanBlock(cancelled,kill,dirtek,bond)==(!cancelled && (!kill||!dirtek&&!bond)),"Protection decision matrix");
  }
  foreach(var role in new[]{"SheriffRole","OfficerRole","JailorRole","FurasRole"}) Check(TouMarmo.Modules.AuditRules.OneMoreChanceRole(role),"Supported OMC role");
  foreach(var role in new[]{"VigilanteRole","DoctorRole","ImpostorRole","CrewmateRole"}) Check(!TouMarmo.Modules.AuditRules.OneMoreChanceRole(role),"Unsupported OMC role");
  for(int alive=0;alive<=15;alive++) {
   Check(TouMarmo.Modules.AuditRules.DoctorImmortal(true,true,alive)==(alive>=3),"Doctor three-player cutoff");
   Check(!TouMarmo.Modules.AuditRules.DoctorImmortal(false,true,alive),"Unrevealed Doctor mortal");
   Check(!TouMarmo.Modules.AuditRules.DoctorImmortal(true,false,alive),"Host disables immortality");
  }
  for(int roll=0;roll<100;roll++) {
   Check(!TouMarmo.Modules.AuditRules.RollSucceeds(roll,0),"0 percent never kills");
   Check(TouMarmo.Modules.AuditRules.RollSucceeds(roll,100),"100 percent always kills");
   Check(TouMarmo.Modules.AuditRules.RollSucceeds(roll,40)==(roll<40),"40 percent roll threshold");
  }
  var bodies=new[]{new Vector2(0,0),new Vector2(.4f,0),new Vector2(.8f,0),new Vector2(2.5f,0)};
  var group=TouMarmo.Modules.BodyGroupRules.Select(bodies,Vector2.Zero,3,3,1.5f);
  Check(group.Length==3 && !group.Contains(3),"Exorcist finds valid subgroup among four bodies");
  Check(TouMarmo.Modules.BodyGroupRules.Select(bodies,Vector2.Zero,3,.5f,1.5f).Length==0,"Required bodies outside radius fail");
  Check(TouMarmo.Modules.BodyGroupRules.Select(new[]{new Vector2(-1,0),new Vector2(1,0)},Vector2.Zero,2,2,1.5f).Length==0,"Pairwise distance required, not just center distance");
  Check(TouMarmo.Modules.BodyGroupRules.Select(bodies,Vector2.Zero,1,3,1.5f).Single()==0,"Nearest single corpse chosen");
  Check(TouMarmo.Modules.BodyGroupRules.Select(bodies,Vector2.Zero,0,3,1.5f).Length==0,"No invalid empty group");
  for(int count=1;count<=5;count++) {
   var points=Enumerable.Range(0,count).Select(i=>new Vector2(i*.2f,0)).ToArray();
   for(int required=1;required<=count;required++) Check(TouMarmo.Modules.BodyGroupRules.Select(points,Vector2.Zero,required,2,1.5f).Length==required,"All close bodies group by required count");
  }
  return checks;
 }
}
"@
Add-Type -TypeDefinition $source
Write-Output "PASS: $([MarmoRuleTests]::Run()) algorithm assertions in memory. No output assembly."
[xml]$locale=Get-Content -Raw (Join-Path $Root 'Resources/Locale/en_US.xml')
$duplicates=@($locale.resources.string | Group-Object name | Where-Object Count -gt 1)
if($duplicates.Count){throw "Duplicate locale keys: $($duplicates.Name)"}
$longIntros=@($locale.resources.string | Where-Object { $_.name -like '*.IntroBlurb' -and $_.'#text'.Length -gt 64 })
if($longIntros.Count){throw "Intro blurbs exceed the role reveal layout: $($longIntros.name -join ', ')"}
foreach($key in @('Role.Bandit','Role.Doctor','Role.Necromancer','Modifier.Simp','Modifier.BigBrain','Modifier.TripleShot')){
 if(!($locale.resources.string | Where-Object name -eq "TouMarmo.$key")){throw "Missing locale: $key"}
}
Write-Output 'PASS: XML, unique locale keys, short intro blurbs and six feature names.'
$files=Get-ChildItem $Root -Recurse -Filter *.cs | Where-Object FullName -notmatch '\\(obj|bin|external)\\'
$ids=@($files | ForEach-Object { $text=Get-Content -Raw $_.FullName; foreach($match in [regex]::Matches($text,'\[MethodRpc\((\d+)')){[int]$match.Groups[1].Value} })
if($ids | Group-Object | Where-Object Count -gt 1){throw 'Duplicate numeric RPC ID'}
if($ids | Where-Object {$_ -in 1,2,3,4,5,101,102,103,104,105}){throw 'RPC collision with MarmoRpc or ComposterRpc'}
Write-Output "PASS: $($ids.Count) numeric RPC IDs unique and disjoint from existing enum IDs."
