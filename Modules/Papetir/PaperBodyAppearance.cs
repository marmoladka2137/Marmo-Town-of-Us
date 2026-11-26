using BepInEx.Logging;
using UnityEngine;
using Object=UnityEngine.Object;
namespace TouMarmo.Modules.Papetir;

// Only paint-chip pixels are rendered. No SpriteMask/full-silhouette layer.
public static class PaperBodyAppearance
{
    private static readonly Dictionary<int, Sprite?> Chips=new();
    private static readonly ManualLogSource Log=BepInEx.Logging.Logger.CreateLogSource("MTOU.PaperAppearance");
    private static Material? chipMaterial;
    public static void Apply(DeadBody body)
    {
        var source=body.bodyRenderers.Where(r=>r && r.sprite)
            .OrderByDescending(r=>r.sprite.bounds.size.x).FirstOrDefault();
        if(!source) return;
        if(source!.GetComponent<PaperBodyPaintView>()) return;
        if(!chipMaterial)
        {
            var shader=Shader.Find("Sprites/Default");
            if(!shader) return;
            chipMaterial=new Material(shader);
        }
        var obj=new GameObject("PaperPaintChips");
        obj.transform.SetParent(source.transform,false);
        obj.layer=source.gameObject.layer;
        obj.transform.localPosition=new Vector3(0,0,-.001f);
        var render=obj.AddComponent<SpriteRenderer>();
        render.sharedMaterial=chipMaterial;render.enabled=false;
        source.gameObject.AddComponent<PaperBodyPaintView>().Configure(source,render);
    }
    public static Sprite? GetChips(Sprite sprite)
    {
        var id=sprite.GetInstanceID();
        if(!Chips.TryGetValue(id,out var chips))
        {
            // Cache each animation frame, including failed/unsupported frames.
            Chips[id]=null;
            try { chips=CreateChips(sprite); Chips[id]=chips; }
            catch(Exception error) { Log.LogWarning($"Paper edge detail unavailable; using normal corpse: {error.Message}"); }
        }
        return chips;
    }
    private static Sprite? CreateChips(Sprite sprite)
    {
        // An atlas rotation from a different game version is a safe no-op.
        if(sprite.packed && sprite.packingRotation!=SpritePackingRotation.None) return null;
        var rect=sprite.textureRect;
        var width=(int)rect.width;var height=(int)rect.height;
        if(width<=0 || height<=0) return null;
        var previous=RenderTexture.active;
        var temporary=RenderTexture.GetTemporary(sprite.texture.width,sprite.texture.height,0,RenderTextureFormat.ARGB32);
        Texture2D? texture=null;
        try
        {
            texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            Graphics.Blit(sprite.texture,temporary);RenderTexture.active=temporary;
            texture.ReadPixels(rect,0,0);texture.Apply();
            var pixels=PaperPaintPixels.Create(texture.GetPixels32().ToArray(),width,height);
            if(!pixels.Any(p=>p.a>0)) return null;
            texture.SetPixels32(pixels);texture.Apply();
            texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var pivot=sprite.pivot-sprite.textureRectOffset;
            var result=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(pivot.x/width,pivot.y/height),sprite.pixelsPerUnit);
            texture=null; // The cached sprite owns the generated texture.
            return result;
        }
        finally
        {
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(temporary);
            if(texture) Object.Destroy(texture);
        }
    }
}
