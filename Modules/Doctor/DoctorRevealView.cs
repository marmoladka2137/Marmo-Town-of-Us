using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Doctor;

/// <summary>Original player cosmetics and a surgical mask following the Mayor reveal animation.</summary>
[RegisterInIl2Cpp]
public sealed class DoctorRevealView(IntPtr ptr) : MonoBehaviour(ptr)
{
    private static readonly Dictionary<int, Rect> Visors = new();
    private static readonly Dictionary<int, Rect> Opaque = new();
    private SpriteRenderer body = null!, front = null!, back = null!, maskImage = null!, coatImage = null!;
    private Transform hat = null!, mask = null!;
    private PoolablePlayer loader = null!;
    private readonly List<Material> materials = new();
    private bool animate, maskReady;
    private float started;
    private Rect maskBounds;
    private Color32[]? bodyPixels;
    private int bodyTexture, bodyTextureWidth;

    [HideFromIl2Cpp]
    public void Configure(PoolablePlayer source, SpriteRenderer bodyImage, bool reveal, PlayerMaterial.MaskType maskType, int maskLayer,
        NetworkedPlayerInfo.PlayerOutfit? appearance)
    {
        body = bodyImage; animate = reveal; started = Time.time;
        hat = new GameObject("Player original hat").transform; hat.SetParent(transform, false);
        mask = new GameObject("Doctor surgical mask").transform; mask.SetParent(transform, false);
        front = CreateImage(hat, "Front", 4); back = CreateImage(hat, "Back", -1);
        maskImage = CreateImage(mask, "Paper mask", 3);
        coatImage = CreateImage(transform, "Player original skin", 1);

        // Native addressable loading, with the source completely hidden (also its colorblind name).
        loader = Object.Instantiate(source, transform);
        loader.name = "Doctor cosmetics source";
        loader.transform.localScale = Vector3.zero;
        loader.gameObject.SetActive(true);
        loader.UpdateFromPlayerOutfit(new NetworkedPlayerInfo.PlayerOutfit
        {
            ColorId = source.ColorId, HatId = appearance?.HatId ?? "hat_NoHat",
            VisorId = DoctorWardrobe.Visor,
            SkinId = appearance?.SkinId ?? source.cosmetics.skin.data?.ProdId ?? "skin_None",
            PetId = string.Empty, PlayerName = string.Empty, NamePlateId = string.Empty
        }, maskType, false, false, null, true);
        loader.SetMaskLayer(maskLayer);
        loader.SetHatAndVisorMaskType(maskType);
        loader.ToggleName(false); loader.TogglePet(false);
        foreach (var text in loader.GetComponentsInChildren<TMP_Text>(true)) text.gameObject.SetActive(false);
    }

    private SpriteRenderer CreateImage(Transform parent, string name, int order)
    {
        var go = new GameObject(name); go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<SpriteRenderer>();
        image.sortingLayerID = body.sortingLayerID;
        image.sortingOrder = body.sortingOrder + order;
        return image;
    }

    private void LateUpdate()
    {
        if (!body || !body.sprite || !loader) return;
        loader.transform.localScale = Vector3.zero;
        if (!maskReady && loader.cosmetics.visor && loader.cosmetics.visor.Image.sprite &&
            loader.cosmetics.visor.Image.sprite.name.Contains("Facemask", StringComparison.OrdinalIgnoreCase))
        {
            CopyImage(loader.cosmetics.visor.Image, maskImage);
            maskBounds = AlphaBounds(maskImage.sprite);
            maskReady = true;
        }
        var visor = VisorBounds(body.sprite);
        var elapsed = animate ? Time.time - started : 10f;
        var skin = loader.cosmetics.skin;
        if (skin && skin.data && skin.data.ProdId != "skin_None" && skin.layer && skin.layer.sprite)
        {
            if (coatImage.sprite != skin.layer.sprite) CopyImage(skin.layer, coatImage);
            var bodyBounds = AlphaBounds(body.sprite);
            var referenceBody = loader.cosmetics.currentBodySprite?.BodySprite;
            var nativeBounds = referenceBody && referenceBody!.sprite ? AlphaBounds(referenceBody.sprite) : bodyBounds;
            // Skin sprites use the player's pivot/coordinates, not their own opaque bounds.
            // Preserve that relationship instead of stretching the coat to a cropped rectangle.
            var scale = bodyBounds.height / Mathf.Max(.01f, nativeBounds.height);
            var offset = new Vector2(bodyBounds.center.x - nativeBounds.center.x * scale,
                bodyBounds.yMin - nativeBounds.yMin * scale);
            coatImage.transform.localScale = Vector3.one * scale;
            coatImage.transform.localPosition = (Vector3)offset + skin.layer.transform.localPosition * scale;
            SetAlpha(coatImage, 1f);
        }
        var maskProgress = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.9f, 1.6f, elapsed));
        var nativeBody = loader.cosmetics.currentBodySprite?.BodySprite;
        if (loader.cosmetics.hat && nativeBody && nativeBody!.sprite)
        {
            var bodyBounds = AlphaBounds(body.sprite);
            var nativeBounds = AlphaBounds(nativeBody.sprite);
            var scale = bodyBounds.height / Mathf.Max(.01f, nativeBounds.height);
            var offset = new Vector2(bodyBounds.center.x - nativeBounds.center.x * scale,
                bodyBounds.yMin - nativeBounds.yMin * scale);
            hat.localPosition = offset;
            hat.localScale = Vector3.one * scale;
            CopyHatLayer(loader.cosmetics.hat.FrontLayer, front);
            CopyHatLayer(loader.cosmetics.hat.BackLayer, back);
        }
        if (maskReady)
        {
            var scale = visor.width * 1.06f / Mathf.Max(.01f, maskBounds.width);
            var target = visor.center + Vector2.down * visor.height * .22f;
            mask.localPosition = target - maskBounds.center * scale;
            mask.localScale = Vector3.one * scale;
            SetAlpha(maskImage, maskProgress);
        }
    }

    private static void SetAlpha(SpriteRenderer image, float alpha) => image.color = new Color(1, 1, 1, alpha);

    private void CopyImage(SpriteRenderer source, SpriteRenderer destination)
    {
        destination.sprite = source.sprite;
        var material = new Material(source.material);
        materials.Add(material); destination.material = material;
        destination.maskInteraction = source.maskInteraction;
    }

    private void CopyHatLayer(SpriteRenderer source, SpriteRenderer destination)
    {
        if (!source || !source.sprite) { destination.enabled = false; return; }
        destination.enabled = source.enabled;
        if (destination.sprite != source.sprite) CopyImage(source, destination);
        destination.transform.localPosition = loader.cosmetics.hat.transform.localPosition + source.transform.localPosition;
        destination.transform.localRotation = source.transform.localRotation;
        destination.transform.localScale = source.transform.localScale;
    }

    [HideFromIl2Cpp]
    private Rect VisorBounds(Sprite sprite)
    {
        if (Visors.TryGetValue(sprite.GetInstanceID(), out var bounds)) return bounds;
        var texture = sprite.texture;
        if (bodyPixels == null || bodyTexture != texture.GetInstanceID())
        {
            bodyPixels = ReadPixels(texture, new Rect(0, 0, texture.width, texture.height));
            bodyTexture = texture.GetInstanceID(); bodyTextureWidth = texture.width;
        }
        var rect = sprite.textureRect;
        var min = new Vector2(float.MaxValue, float.MaxValue); var max = new Vector2(float.MinValue, float.MinValue);
        for (int y = 0; y < (int)rect.height; y++) for (int x = 0; x < (int)rect.width; x++)
        {
            var color = bodyPixels[((int)rect.y + y) * bodyTextureWidth + (int)rect.x + x];
            // AU's player sprites encode visor pixels in green, independently of player color.
            if (color.a < 40 || color.g < 45 || color.g <= color.r * 1.35f || color.g <= color.b * 1.35f) continue;
            min = Vector2.Min(min, new Vector2(x, y)); max = Vector2.Max(max, new Vector2(x + 1, y + 1));
        }
        bounds = min.x == float.MaxValue ? new Rect(.35f, -.05f, .8f, .55f) : LocalBounds(sprite, min, max);
        Visors[sprite.GetInstanceID()] = bounds;
        return bounds;
    }

    [HideFromIl2Cpp]
    private static Rect AlphaBounds(Sprite sprite)
    {
        if (Opaque.TryGetValue(sprite.GetInstanceID(), out var cached)) return cached;
        var rect = sprite.textureRect; var pixels = ReadPixels(sprite.texture, rect);
        var min = new Vector2(float.MaxValue, float.MaxValue); var max = new Vector2(float.MinValue, float.MinValue);
        for (int y = 0; y < (int)rect.height; y++) for (int x = 0; x < (int)rect.width; x++)
        {
            if (pixels[y * (int)rect.width + x].a < 40) continue;
            min = Vector2.Min(min, new Vector2(x, y)); max = Vector2.Max(max, new Vector2(x + 1, y + 1));
        }
        var bounds = min.x == float.MaxValue ? new Rect(-.5f, -.5f, 1, 1) : LocalBounds(sprite, min, max);
        Opaque[sprite.GetInstanceID()] = bounds; return bounds;
    }

    [HideFromIl2Cpp]
    private static Rect LocalBounds(Sprite sprite, Vector2 min, Vector2 max)
    {
        var offset = sprite.textureRectOffset - sprite.pivot;
        min = (min + offset) / sprite.pixelsPerUnit; max = (max + offset) / sprite.pixelsPerUnit;
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    [HideFromIl2Cpp]
    private static Color32[] ReadPixels(Texture2D source, Rect rect)
    {
        var previous = RenderTexture.active;
        var render = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        var readable = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        try
        {
            Graphics.Blit(source, render); RenderTexture.active = render;
            readable.ReadPixels(rect, 0, 0); readable.Apply();
            return readable.GetPixels32().ToArray();
        }
        finally
        {
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(render); Object.Destroy(readable);
        }
    }

    private void OnDestroy()
    {
        bodyPixels = null;
        foreach (var material in materials) if (material) Object.Destroy(material);
    }
}

