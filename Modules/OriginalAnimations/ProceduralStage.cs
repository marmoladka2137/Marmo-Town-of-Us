using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.OriginalAnimations;

/// <summary>Original geometry made from Unity's built-in white texture, without imported frames.</summary>
internal sealed class ProceduralStage : IDisposable
{
    private static Sprite? pixel;
    private readonly Transform root;
    private readonly Material material;
    private readonly int order;
    private int drawIndex;
    public ProceduralStage(Transform root, int order = 100)
    {
        this.root = root; this.order = order;
        material = new Material(Shader.Find("Sprites/Default"));
        if (!pixel)
        {
            var white = Texture2D.whiteTexture;
            pixel = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(.5f, .5f), white.width);
            pixel.name = "MTOU procedural unit square";
        }
    }
    public Transform Group(string name, Transform? parent = null)
    {
        var go = new GameObject(name); go.layer = root.gameObject.layer;
        go.transform.SetParent(parent ?? root, false); return go.transform;
    }
    public SpriteRenderer Quad(string name, Vector2 center, Vector2 size, Color color, Transform? parent = null)
    {
        var t = Group(name, parent); t.localPosition = new Vector3(center.x, center.y, 0);
        t.localScale = new Vector3(size.x, size.y, 1);
        var image = t.gameObject.AddComponent<SpriteRenderer>(); image.sprite = pixel;
        image.sharedMaterial = material; image.color = color; image.sortingOrder = order + drawIndex++;
        return image;
    }
    public Transform Cross(string name, Vector2 center, float size, Color color, Transform? parent = null)
    {
        var group = Group(name, parent); group.localPosition = center;
        Quad("vertical", Vector2.zero, new Vector2(size * .3f, size), color, group);
        Quad("horizontal", Vector2.zero, new Vector2(size, size * .3f), color, group);
        return group;
    }
    public Transform Ring(string name, float radius, float thickness, Color color, Transform? parent = null)
    {
        var group = Group(name, parent);
        const int segments = 40;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2 / segments;
            var image = Quad("arc", new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                new Vector2(radius * Mathf.PI * 2 / segments + thickness * .2f, thickness), color, group);
            image.transform.localEulerAngles = new Vector3(0, 0, a * Mathf.Rad2Deg + 90);
        }
        return group;
    }
    public static void Alpha(Transform group, float alpha)
    {
        foreach (var image in group.GetComponentsInChildren<SpriteRenderer>(true))
        { var color = image.color; color.a = Mathf.Clamp01(alpha); image.color = color; }
    }
    public static void NonInteractive(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
        foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var b in go.GetComponentsInChildren<PassiveButton>(true)) b.enabled = false;
        foreach (var text in go.GetComponentsInChildren<TMP_Text>(true)) text.gameObject.SetActive(false);
    }
    public void Dispose() { if (material) Object.Destroy(material); }
}
