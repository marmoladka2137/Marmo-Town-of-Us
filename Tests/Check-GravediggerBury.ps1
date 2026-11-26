param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$raw=Get-Content (Join-Path $Root 'Modules/Gravedigger/GravediggerRuntime.cs') -Raw
$start=$raw.IndexOf('public static bool CanBury(');$end=$raw.IndexOf(';',$start)
$method=$raw.Substring($start,$end-$start+1)
$ledger=(Get-Content (Join-Path $Root 'Modules/Gravedigger/BurialLedger.cs') -Raw).Replace('namespace TouMarmo.Modules.Gravedigger;','namespace TouMarmo.Modules.Gravedigger {')+'}'
$code=@'
#nullable enable
using System;using System.Linq;using System.Collections.Generic;using System.Numerics;using TouMarmo.Modules.Gravedigger;
public class Obj {public static implicit operator bool(Obj? o)=>o!=null;}
public class Role{}public class GravediggerRole:Role{}public class Data{public Role Role=new GravediggerRole();}
public class DisabledModifier{public bool CanUseAbilities;}
public class PlayerControl:Obj{public Data Data=new();public bool moveable=true,inVent,Dead;public float Delay;public Vector2 Position;public List<DisabledModifier> Mods=new();public List<DisabledModifier> GetModifiers<T>()=>Mods;public Vector2 GetTruePosition()=>Position;public bool HasDied()=>Dead;}
public class BodyObject{public bool activeInHierarchy=true;}
public class DeadBody:Obj{public byte ParentId=1;public bool Reported,Paper;public BodyObject gameObject=new();public Vector2 TruePosition=new Vector2(1,0);}
public static class MarmoSession{public static bool InRound=true;public static PlayerControl? Victim;public static PlayerControl? Player(byte id)=>Victim;public static bool Alive(PlayerControl? p)=>p!=null&&!p.Dead;}
public static class TimeLordRewindSystem{public static bool IsRewinding;}
namespace Necromancer{public class S{public bool Active;public byte BodyId;}public static class NecromancerRuntime{public static Dictionary<byte,S> States=new();}}
namespace Furas{public static class FurasRuntime{public static bool Open=true;public static bool ClearPath(Vector2 a,Vector2 b)=>Open;}}
public class State{public BurialLedger Ledger=new();}
namespace Papetir{public static class PapetirRuntime{public static bool IsPaper(DeadBody body)=>body.Paper;}}
public static class Runtime{public static HashSet<byte> removed=new();public static State State=new();public static State For(PlayerControl p)=>State;public static float Remaining(PlayerControl p)=>p.Delay;
'@
$code+=$method+'}'+$ledger
$code+=@'
public static class Tests {static int n;static void Check(bool b,string s){n++;if(!b)throw new Exception(s);}public static int Run(){
 var p=new PlayerControl();var body=new DeadBody();Check(Runtime.CanBury(p,body),"Valid nearby corpse");
 MarmoSession.Victim=new PlayerControl();Check(!Runtime.CanBury(p,body),"Revived player's stale body rejected");MarmoSession.Victim=null;
 body.Paper=true;Check(!Runtime.CanBury(p,body),"Paper corpse cannot grant a real role");body.Paper=false;
 Check(!Runtime.CanBury(p,null),"Missing body rejected");p.Dead=true;Check(!Runtime.CanBury(p,body),"Dead owner rejected");p.Dead=false;
 p.Data.Role=new Role();Check(!Runtime.CanBury(p,body),"Wrong role rejected");p.Data.Role=new GravediggerRole();
 MarmoSession.InRound=false;Check(!Runtime.CanBury(p,body),"Meeting and intro rejected");MarmoSession.InRound=true;
 p.moveable=false;Check(!Runtime.CanBury(p,body),"Disabled movement rejected");p.moveable=true;
 p.inVent=true;Check(!Runtime.CanBury(p,body),"Venting rejected");p.inVent=false;
 TimeLordRewindSystem.IsRewinding=true;Check(!Runtime.CanBury(p,body),"Rewind rejected");TimeLordRewindSystem.IsRewinding=false;
 p.Delay=1;Check(!Runtime.CanBury(p,body),"Host cooldown enforced");p.Delay=0;
 p.Mods.Add(new DisabledModifier());Check(!Runtime.CanBury(p,body),"Disabled ability rejected");p.Mods.Clear();
 body.Reported=true;Check(!Runtime.CanBury(p,body),"Reported body rejected");body.Reported=false;
 body.gameObject.activeInHierarchy=false;Check(!Runtime.CanBury(p,body),"Inactive body rejected");body.gameObject.activeInHierarchy=true;
 Runtime.removed.Add(1);Check(!Runtime.CanBury(p,body),"Already accepted burial rejected");Runtime.removed.Clear();
 Necromancer.NecromancerRuntime.States[2]=new(){Active=true,BodyId=1};Check(!Runtime.CanBury(p,body),"Controlled corpse cannot be buried");Necromancer.NecromancerRuntime.States.Clear();
 Furas.FurasRuntime.Open=false;Check(!Runtime.CanBury(p,body),"Corpse behind wall rejected");Furas.FurasRuntime.Open=true;
 body.TruePosition=new Vector2(1.5f,0);Check(Runtime.CanBury(p,body),"Exact range accepted");body.TruePosition=new Vector2(1.501f,0);Check(!Runtime.CanBury(p,body),"Out of range rejected");body.TruePosition=new Vector2(1,0);
 Runtime.State.Ledger.Bury(1,7);Check(!Runtime.CanBury(p,body),"Same victim cannot be farmed");Runtime.State.Ledger.Inherit(7,99);Check(!Runtime.CanBury(p,new DeadBody{ParentId=2}),"Inherited role has no Bury");return n;
}}
'@
Add-Type -TypeDefinition $code -WarningAction SilentlyContinue
"PASS $([Tests]::Run()) production CanBury checks with API doubles; not multiplayer."
