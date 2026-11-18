using HarmonyLib;
using MiraAPI.Patches.Options;
using UnityEngine;

namespace TouMarmo.Modules;

// Use the native TOU menu; only fit our artwork into its existing icon slot.
[HarmonyPatch(typeof(RoleSettingMenuPatches), "CreateQuotaOption")]
public static class MarmoMenuIcons
{
    private static readonly Dictionary<int, Sprite> FittedIcons = new();

    private static Sprite FitVisibleArtwork(Sprite original)
    {
        var key = original.GetInstanceID();
        if (FittedIcons.TryGetValue(key, out var cached) && cached) return cached;
        var texture = original.texture;
        if (!texture.isReadable) return original;
        var pixels = texture.GetPixels32();
        var rect = original.rect;
        var minX = (int)rect.xMax;
        var minY = (int)rect.yMax;
        var maxX = -1;
        var maxY = -1;
        for (var y = (int)rect.y; y < (int)rect.yMax; y++)
        for (var x = (int)rect.x; x < (int)rect.xMax; x++)
        {
            if (pixels[y * texture.width + x].a < 16) continue;
            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }
        if (maxX < minX || maxY < minY) return original;
        var fitted = Sprite.Create(texture,
            new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
            new Vector2(0.5f, 0.5f), original.pixelsPerUnit);
        FittedIcons[key] = fitted;
        return fitted;
    }

    [HarmonyPostfix]
    public static void Postfix(RoleBehaviour role, RoleOptionSetting? __result)
    {
        if (__result == null || role.GetType().Assembly != typeof(TouMarmoPlugin).Assembly) return;
        var icon = __result.transform.Find("RoleIcon");
        if (!icon) return;
        var renderer = icon.GetComponent<SpriteRenderer>();
        if (!renderer || !renderer.sprite) return;
        renderer.sprite = FitVisibleArtwork(renderer.sprite);
        var size = renderer.sprite.bounds.size;
        var extent = Mathf.Max(size.x, size.y);
        if (extent <= 0f) return;
        // Native rows are 0.43 units high. Leave a margin and preserve aspect ratio.
        var scale = 0.36f / extent;
        icon.localScale = new Vector3(scale, scale, 1f);
        icon.localPosition = new Vector3(-1.3f, -0.3f, -2f);
    }
}
