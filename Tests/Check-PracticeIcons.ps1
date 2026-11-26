param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Patches/AbilityVisualSizes.cs') -Raw
$a=$part.IndexOf('    public static bool IsPracticeTool');$b=$part.IndexOf('    [HarmonyPostfix',$a)
$code='using System;using System.Collections.Generic;using System.Linq;public class ActionButton{}public class CustomActionButton{public ActionButton Button;}public static class Selection {'+$part.Substring($a,$b-$a)+'}'
$code+=@'
namespace TownOfUs.Buttons.BaseFreeplay {
 public class RemoteKillButton:CustomActionButton{}public class FreeplayResetButton:CustomActionButton{}
 public class FreeplaySetRolesButton:CustomActionButton{}public class FreeplaySetModifiersButton:CustomActionButton{}
}
public static class Tests{
 static int n;static void Check(bool ok,string name){n++;if(!ok)throw new Exception(name);}
 public static int Run(){
  var tools=new CustomActionButton[]{new TownOfUs.Buttons.BaseFreeplay.RemoteKillButton(),new TownOfUs.Buttons.BaseFreeplay.FreeplayResetButton(),new TownOfUs.Buttons.BaseFreeplay.FreeplaySetRolesButton(),new TownOfUs.Buttons.BaseFreeplay.FreeplaySetModifiersButton()};
  foreach(var t in tools)t.Button=new ActionButton();
  var role=new CustomActionButton{Button=new ActionButton()};var native=new ActionButton();
  var selected=Selection.SelectButtons(tools.Append(role),tools.Select(t=>t.Button).Append(native).Append(role.Button)).ToArray();
  foreach(var t in tools){Check(Selection.IsPracticeTool(t.GetType()),"Practice type recognized");Check(!selected.Contains(t.Button),"Practice tool stays excluded even through native HUD enumeration");}
  Check(selected.Contains(role.Button),"Role ability remains selected");Check(selected.Contains(native),"Native gameplay button remains selected");
  Check(selected.Count(b=>b==role.Button)==1,"Duplicate enumeration handled once");
  Check(Selection.SelectButtons(new[]{new CustomActionButton()},Array.Empty<ActionButton>()).Count()==0,"Not-yet-created button safely ignored");
  return n;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([Tests]::Run()) production button-selection assertions; no game launched."
if($part -match 'Sprite\.Create|b\.graphic\.sprite\s*='){throw 'Sprite replacement reintroduced'}
if($part -match '\b(?:Timer|Cooldown|UsesLeft|EffectDuration)\s*='){throw 'Gameplay values modified by visual patch'}
'PASS: original sprite references and gameplay timers are not assigned by the visual patch.'
