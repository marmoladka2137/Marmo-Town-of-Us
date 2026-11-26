param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$source=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Modules/ModifierAutomation.cs') -Raw
$production=$source.Replace('namespace TownOfUs.Modules;','namespace TownOfUs.Modules {')+'}'
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;
using TownOfUs.Modules;using TownOfUs.Modifiers.Game;using TownOfUs.Modifiers.Game.Assailant;using TownOfUs.Modifiers.Game.Crewmate;
namespace MiraAPI.Modifiers{public class BaseModifier{}}
namespace TownOfUs.Modifiers.Game{public class TouGameModifier:MiraAPI.Modifiers.BaseModifier{}public class NormalCrewModifier:TouGameModifier{}public class NormalImpostorModifier:TouGameModifier{}}
namespace TownOfUs.Modifiers.Game.Assailant{public class AssassinModifier:TouGameModifier{}public class DoubleShotModifier:TouGameModifier{}}
namespace TownOfUs.Modifiers.Game.Crewmate{public class TestCleanModifier:TouGameModifier{}public class TestTimeLordModifier:TouGameModifier{}}
'@
# Keep using directives before the stub namespaces.
$production=$production.Replace('using MiraAPI.Modifiers;','').Replace('using TownOfUs.Modifiers.Game;','').Replace('using TownOfUs.Modifiers.Game.Assailant;','').Replace('using TownOfUs.Modifiers.Game.Crewmate;','')
$code="using MiraAPI.Modifiers;`n"+$code.Replace('#nullable enable',"#nullable enable`n")+$production
$code+=@'
public static class ModifierTests{
 static int n;static void Check(bool pass,string name){n++;if(!pass)throw new Exception(name);}
 public static int Run(){
  var clean=new TestCleanModifier();var rewind=new TestTimeLordModifier();var crew=new NormalCrewModifier();var imp=new NormalImpostorModifier();var assassin=new AssassinModifier();var doubleShot=new DoubleShotModifier();
  Check(ModifierAutomation.IsDiagnostic(clean),"Test Clean classified as diagnostic");
  Check(ModifierAutomation.IsDiagnostic(rewind),"Test Time Lord classified as diagnostic");
  Check(!ModifierAutomation.IsDiagnostic(crew)&&!ModifierAutomation.IsDiagnostic(imp),"Real faction modifiers preserved");
  var dummyPool=new TouGameModifier[]{clean,crew,rewind,imp}.Where(m=>!ModifierAutomation.IsDiagnostic(m)).ToArray();
  Check(dummyPool.SequenceEqual(new[]{crew,(TouGameModifier)imp}),"Random Practice loadout excludes debug abilities");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{clean},false)==null,"Amnesiac remembering an Imitator with Test Clean cannot inherit Clean");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{clean,rewind},true)==null,"Diagnostic abilities never transfer even with Assassin enabled");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{clean,crew},false)==crew,"Debug modifier cannot mask a legitimate inherited modifier");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{rewind,imp},false)==imp,"Legitimate impostor faction inheritance preserved");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{assassin},true)==null,"Assassin still handled by existing separate option logic");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{doubleShot},false)==null,"Double Shot still requires Assassin");
  Check(ModifierAutomation.RememberedFactionModifier(new TouGameModifier[]{doubleShot},true)==doubleShot,"Existing enabled Double Shot behavior retained");
  Check(ModifierAutomation.RememberedFactionModifier(Array.Empty<TouGameModifier>(),false)==null,"Empty corpse modifiers safe");
  return n;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([ModifierTests]::Run()) production modifier-selection checks with API doubles."
$amne=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Roles/Classic/Neutral/NeutralBenign/AmnesiacRole.cs') -Raw
$dummy=Get-Content (Join-Path $Root 'IntegratedTownOfUs/Patches/DummyBehaviourPatch.cs') -Raw
if($amne -notmatch 'ModifierAutomation.RememberedFactionModifier' -or $amne -notmatch 'opts.InheritFactionModifier && modifier != null'){throw 'Amnesiac must use the policy and retain the host option'}
if($dummy -notmatch '!ModifierAutomation.IsDiagnostic\(x\)'){throw 'Practice random selection must reject diagnostic modifiers'}
if($amne -notmatch 'player.ChangeRole\(\(ushort\)roleWhenAlive.Role\)'){throw 'Remembered role must remain the corpse role'}
'PASS real Remember/dummy call sites use the policy; role assignment and host inheritance setting preserved. Not an in-game test.'
