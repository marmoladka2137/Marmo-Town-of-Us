param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$part=Get-Content (Join-Path $Root 'Modules/Manipulator/ManipulatorAudio.cs') -Raw
$part=$part.Replace('using UnityEngine;','').Replace('namespace TouMarmo.Modules.Manipulator;','namespace TouMarmo.Modules.Manipulator {')
$code=@'
#nullable enable annotations
using System; using UnityEngine; using static FakeLog;
public static class FakeLog {public static void Warning(string s){}}
namespace UnityEngine {public class AudioClip {public static implicit operator bool(AudioClip c)=>c!=null;}}
public class LoadableAudioResourceAsset(string path) {public AudioClip LoadAsset(){if(!path.EndsWith("Manipulated.wav"))throw new Exception();return new AudioClip();}}
public static class Constants {public static bool Sfx=true;public static bool ShouldPlaySfx()=>Sfx;}
public class SoundManager {
 public static SoundManager Instance=new();public int Starts,Stops;public bool Loop;public AudioClip LastStarted,LastStopped;
 public static implicit operator bool(SoundManager m)=>m!=null;
 public void PlaySound(AudioClip c,bool loop,float volume){Starts++;LastStarted=c;Loop=loop;}
 public void StopSound(AudioClip c){Stops++;LastStopped=c;}
}
'@
$code+=$part+'}'
$code+=@'
public static class AudioTests {
 static int n;static void Check(bool value,string name){if(!value)throw new Exception(name);n++;}
 public static int Run(){
  var m=SoundManager.Instance;
  TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(false);Check(m.Starts==0,"Inactive effect is silent");
  TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(true);Check(m.Starts==1&&m.Loop,"Effect starts loop");
  for(int i=0;i<1000;i++)TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(true);
  Check(m.Starts==1,"Frames do not duplicate playback");
  TouMarmo.Modules.Manipulator.ManipulatorAudio.Stop();Check(m.Stops==1,"Cleanup stops immediately");
  Check(ReferenceEquals(m.LastStarted,m.LastStopped),"Cleanup stops only owned clip");
  TouMarmo.Modules.Manipulator.ManipulatorAudio.Stop();Check(m.Stops==1,"Repeated cleanup is idempotent");
  Constants.Sfx=false;TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(true);Check(m.Starts==1,"SFX mute respected");
  Constants.Sfx=true;TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(true);Check(m.Starts==2,"Next effect can restart");
  TouMarmo.Modules.Manipulator.ManipulatorAudio.Update(false);Check(m.Stops==2,"Inactive state removes restarted loop");
  return n;
 }
}
'@
Add-Type -TypeDefinition $code -Language CSharp
"PASS $([AudioTests]::Run()) production audio lifecycle checks with API doubles; not an audible Unity test."
