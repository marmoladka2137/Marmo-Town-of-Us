using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TouMarmo.Modules.OriginalAnimations;

/// <summary>New synthesis, with no recorded or imported audio.</summary>
internal static class PresentationSound
{
    private static AudioClip? doctor, deputy;
    public static void Play(bool medical)
    {
        if (!SoundManager.Instance) return;
        var clip = medical ? doctor : deputy;
        if (!clip)
        {
            const int rate = 24000;
            var samples = new Il2CppStructArray<float>((int)(rate * (medical ? .65f : .20f)));
            uint noise = 17491; // Local PRNG: does not advance gameplay randomness.
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                noise = unchecked(noise * 1664525u + 1013904223u);
                float n = (noise >> 8) / 8388608f - 1;
                samples[i] = medical
                    ? .19f * Mathf.Sin(2 * Mathf.PI * (1150 * t - 430 * t * t)) * Mathf.Exp(-11 * t) * Mathf.Min(1, t * 250)
                      + (t > .19f ? .12f * Mathf.Sin(2 * Mathf.PI * 1460 * (t - .19f)) * Mathf.Exp(-16 * (t - .19f)) : 0)
                    : (.22f * n + .18f * Mathf.Sin(2 * Mathf.PI * 95 * t)) * Mathf.Exp(-32 * t) * Mathf.Min(1, t * 1500);
            }
            clip = AudioClip.Create(medical ? "MTOU original water chime" : "MTOU original shot", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            if (medical) doctor = clip; else deputy = clip;
        }
        SoundManager.Instance.PlaySound(clip!, false, .8f);
    }
}
