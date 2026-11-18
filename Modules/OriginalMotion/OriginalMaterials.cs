using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TouMarmo.Modules.OriginalMotion;

/// <summary>No redistributed GPU programs. Fonts reference the installed game; fills are original pixels.</summary>
internal static class OriginalMaterials
{
    private static readonly List<UnityEngine.Object> Lifetime = new();
    internal static void Bind(AssetBundle bundle)
    {
        // The generic LoadAllAssets<T> helper is stripped in the target game. Use the native overload.
        foreach (var item in bundle.LoadAllAssets(Il2CppType.Of<Material>()))
        {
            var material = item.Cast<Material>();
            if (material.name.StartsWith("LiberationSans", StringComparison.Ordinal))
            {
                material.shader = Shader.Find("TextMeshPro/Mobile/Distance Field")
                    ?? Shader.Find("TextMeshPro/Distance Field")
                    ?? throw new InvalidOperationException("The installed game has no TMP distance-field shader.");
                material.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                Lifetime.Add(material);
            }
            else if (material.name is "trap" or "ignite" or "bomb" or "eclipse")
            {
                material.shader = Shader.Find("Sprites/Default")
                    ?? throw new InvalidOperationException("The installed game has no sprite shader.");
                material.mainTexture = Fill(material.name);
                material.color = Color.white;
                material.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                Lifetime.Add(material);
            }
        }
    }
    private static Texture2D Fill(string name)
    {
        const int size = 128;
        var pixels = new Il2CppStructArray<Color32>(size * size);
        Color32 accent = name switch
        {
            "trap" => new Color32(255, 190, 55, 185),
            "ignite" => new Color32(245, 110, 40, 185),
            "bomb" => new Color32(235, 60, 80, 185),
            _ => new Color32(190, 120, 245, 175)
        };
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            if (name == "eclipse")
            {
                float radius = Mathf.Sqrt(Mathf.Pow((x - 63.5f) / 62f, 2) + Mathf.Pow((y - 63.5f) / 62f, 2));
                float alpha = Mathf.Clamp01((1 - radius) * 15);
                pixels[y * size + x] = new Color32(accent.r, accent.g, accent.b, (byte)(accent.a * alpha * (radius > .88f ? 1 : .2f)));
            }
            else pixels[y * size + x] = ((x + y) / 12) % 2 == 0 ? accent : new Color32(25, 30, 45, 110);
        }
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "MTOU original surface " + name;
        texture.SetPixels32(pixels); texture.Apply(false, true);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        Lifetime.Add(texture);
        return texture;
    }
}
