using System.Text.Json;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace TouMarmo.Modules.OriginalMotion;

public sealed class MotionSpec
{
    public string Name { get; set; } = "";
    public float Duration { get; set; }
    public bool Loop { get; set; }
    public int Frames { get; set; }
    public string[] SpritePaths { get; set; } = new[] { "" };
}
public sealed class SoundSpec
{
    public string Name { get; set; } = "";
    public float Duration { get; set; }
}
internal sealed class MotionCatalogData
{
    public MotionSpec[] Clips { get; set; } = Array.Empty<MotionSpec>();
    public SoundSpec[] Audio { get; set; } = Array.Empty<SoundSpec>();
}

/// <summary>Only IDs from this mod's cleaned resource bundle enter the interception table.</summary>
public static class OriginalMotionCatalog
{
    private static readonly MotionCatalogData Data = ReadCatalog();
    private static readonly Dictionary<int, MotionSpec> Registered = new();
    private static readonly List<AnimationClip> ClipLifetime = new();
    private static readonly Dictionary<string, Sprite[]> Frames = new(StringComparer.Ordinal);
    public static IReadOnlyList<MotionSpec> Specs => Data.Clips;
    public static IReadOnlyList<SoundSpec> Sounds => Data.Audio;

    private static MotionCatalogData ReadCatalog()
    {
        using var stream = typeof(OriginalMotionCatalog).Assembly.GetManifestResourceStream("TouMarmo.Resources.OriginalMotion.catalog.json")
            ?? throw new InvalidOperationException("Original motion catalog is missing.");
        return JsonSerializer.Deserialize<MotionCatalogData>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Invalid original motion catalog.");
    }

    public static AssetBundle RegisterBundle(AssetBundle bundle)
    {
        OriginalMaterials.Bind(bundle);
        foreach (var spec in Data.Clips)
        {
            var clip = bundle.LoadAsset<AnimationClip>(spec.Name);
            if (!clip) throw new InvalidOperationException("Clean bundle is missing motion interface: " + spec.Name);
            clip.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            ClipLifetime.Add(clip);
            // This is an owned timing envelope, not a copied visual curve. It is never played by Animator.
            clip.legacy = true; clip.ClearCurves(); clip.frameRate = 24;
            clip.wrapMode = spec.Loop ? WrapMode.Loop : WrapMode.Once;
            clip.SetCurve("", Il2CppType.Of<Transform>(), "localPosition.x", AnimationCurve.Linear(0, 0, spec.Duration, 0));
            if (Math.Abs(clip.length - spec.Duration) > .001f)
                throw new InvalidOperationException("Original clip duration mismatch: " + spec.Name);
            Registered[clip.GetInstanceID()] = spec;
        }
        if (Data.Clips.Any(x => !Registered.Values.Any(y => y.Name == x.Name)))
            throw new InvalidOperationException("Clean bundle is missing original motion interfaces.");
        return bundle;
    }

    public static bool Find(AnimationClip? clip, out MotionSpec spec)
    { spec = null!; return clip && Registered.TryGetValue(clip!.GetInstanceID(), out spec!); }

    public static Sprite Frame(MotionSpec spec, int index)
    {
        if (!Frames.TryGetValue(spec.Name, out var frames)) Frames[spec.Name] = frames = new Sprite[spec.Frames];
        index = Math.Clamp(index, 0, frames.Length - 1);
        if (frames[index]) return frames[index];
        string name = $"TouMarmo.Resources.OriginalMotion.{spec.Name}.{index:D3}.png";
        using var stream = typeof(OriginalMotionCatalog).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException("Missing original frame: " + name);
        using var bytes = new MemoryStream(); stream.CopyTo(bytes);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(texture, bytes.ToArray(), true))
            throw new InvalidOperationException("Could not decode original frame: " + name);
        texture.name = $"MTOU original {spec.Name} {index}";
        texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        texture.filterMode = FilterMode.Bilinear; texture.wrapMode = TextureWrapMode.Clamp;
        frames[index] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 128,
            0, SpriteMeshType.FullRect);
        frames[index].name = texture.name;
        frames[index].hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return frames[index];
    }
}
