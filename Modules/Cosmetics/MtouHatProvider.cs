using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
namespace TouMarmo.Modules.Cosmetics;
public sealed class MtouHatProvider : ResourceProviderBase
{
    public MtouHatProvider(IntPtr ptr) : base(ptr) { }
    public MtouHatProvider() : base(ClassInjector.DerivedConstructorPointer<MtouHatProvider>()) => ClassInjector.DerivedConstructorBody(this);
    public override void Provide(ProvideHandle handle)
    {
        Sprite? sprite = null;
        try
        {
            var entry = HeadHatCollection.Heads.First(h => h.AssetId == handle.Location.PrimaryKey);
            sprite = HeadHatCollection.LoadSprite(entry);
            if (handle.Type.FullName == Il2CppType.Of<PreviewViewData>().FullName)
            {
                var view = ScriptableObject.CreateInstance<PreviewViewData>(); view.PreviewSprite = sprite;
                handle.Complete(view,true,null);
            }
            else
            {
                var view = ScriptableObject.CreateInstance<HatViewData>(); view.MainImage = sprite; view.MatchPlayerColor = false;
                handle.Complete(view,true,null);
            }
        }
        catch (Exception error)
        {
            HeadHatCollection.ReleaseSprite(sprite);
            handle.Complete<UnityEngine.Object>(null!,false,new Il2CppSystem.Exception(error.Message));
        }
    }
    public override void Release(IResourceLocation location, Il2CppSystem.Object asset)
    {
        if (asset == null) return;
        if (asset.TryCast<HatViewData>() is { } hat) { HeadHatCollection.ReleaseSprite(hat.MainImage); UnityEngine.Object.Destroy(hat); }
        else if (asset.TryCast<PreviewViewData>() is { } preview) { HeadHatCollection.ReleaseSprite(preview.PreviewSprite); UnityEngine.Object.Destroy(preview); }
    }
}
public sealed class MtouHatLocator : Il2CppSystem.Object
{
    private readonly Dictionary<string, Il2CppSystem.Collections.Generic.IList<IResourceLocation>> locations = new();
    private readonly Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> keys = new();
    public MtouHatLocator(IntPtr ptr) : base(ptr) { }
    public MtouHatLocator() : base(ClassInjector.DerivedConstructorPointer<MtouHatLocator>()) => ClassInjector.DerivedConstructorBody(this);
    public string LocatorId => "MTOU.HeadHats";
    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> Keys => new(keys.Pointer);
    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out Il2CppSystem.Collections.Generic.IList<IResourceLocation> result)
    {
        result = null!;
        var id = key?.ToString();
        if (id == null || !HeadHatCollection.Heads.Any(h => h.AssetId == id) ||
            type.FullName != Il2CppType.Of<HatViewData>().FullName && type.FullName != Il2CppType.Of<PreviewViewData>().FullName) return false;
        var cache = id + type.FullName;
        if (locations.TryGetValue(cache, out var found)) { result = found; return true; }
        var list = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        list.Add(new ResourceLocationBase(id,id,typeof(MtouHatProvider).FullName,type));
        result = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(list.Pointer);
        locations[cache] = result; keys.Add(key!); return true;
    }
}

