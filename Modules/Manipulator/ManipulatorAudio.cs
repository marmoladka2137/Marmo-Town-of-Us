using UnityEngine;
namespace TouMarmo.Modules.Manipulator;

// Unique clip and owned loop: stopping this effect never stops other roles' audio.
public static class ManipulatorAudio
{
    private static readonly LoadableAudioResourceAsset Sound = new("TouMarmo.Resources.Manipulator.Manipulated.wav");
    private static AudioClip? clip;
    private static bool playing, failed;
    public static void Update(bool active)
    {
        if(!active) { Stop(); return; }
        if(playing || failed || !SoundManager.Instance || !Constants.ShouldPlaySfx()) return;
        try
        {
            clip=Sound.LoadAsset();
            SoundManager.Instance.PlaySound(clip,true,1f);
            playing=true;
        }
        catch(Exception error)
        {
            failed=true;
            Warning($"Manipulator audio could not start: {error.Message}");
        }
    }
    public static void Stop()
    {
        if(playing && clip && SoundManager.Instance) SoundManager.Instance.StopSound(clip!);
        playing=false; failed=false;
    }
}
