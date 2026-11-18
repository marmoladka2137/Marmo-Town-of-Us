using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TouMarmo.Modules.OriginalMotion;

/// <summary>New signal synthesis, no original recordings/samples; preserves cue names and duration.</summary>
public sealed class OriginalAudioAsset(string name) : LoadableAsset<AudioClip>
{
    private static readonly Dictionary<string, AudioClip> Cache = new(StringComparer.Ordinal);
    public override AudioClip LoadAsset()
    {
        if (LoadedAsset) return LoadedAsset!;
        if (Cache.TryGetValue(name, out var cached) && cached) return LoadedAsset = cached;
        var spec = OriginalMotionCatalog.Sounds.FirstOrDefault(x => x.Name == name)
            ?? throw new InvalidOperationException("Missing original sound contract: " + name);
        const int rate = 22050;
        int count = Math.Max(1, (int)Math.Round(spec.Duration * rate));
        var data = new Il2CppStructArray<float>(count);
        uint hash = 2166136261;
        foreach (char c in name) hash = unchecked((hash ^ c) * 16777619);
        float frequency = 340 + hash % 500;
        bool alarm = name.Contains("Alarm", StringComparison.OrdinalIgnoreCase) || name.Contains("Heart", StringComparison.OrdinalIgnoreCase);
        for (int i = 0; i < count; i++)
        {
            float time = (float)i / rate;
            float phase = alarm ? time % .42f : time % .65f;
            float envelope = Mathf.Min(1, phase * 150) * Mathf.Exp(-9 * phase) * Mathf.Min(1, (count - i) / (rate * .025f));
            float tone = Mathf.Sin(2 * Mathf.PI * frequency * time) + .22f * Mathf.Sin(2 * Mathf.PI * frequency * 1.5f * time);
            data[i] = .18f * envelope * tone;
        }
        var clip = AudioClip.Create(name, count, 1, rate, false);
        clip.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        if (!clip.SetData(data, 0)) throw new InvalidOperationException("Could not generate original sound: " + name);
        Cache[name] = clip;
        return LoadedAsset = clip;
    }
}
