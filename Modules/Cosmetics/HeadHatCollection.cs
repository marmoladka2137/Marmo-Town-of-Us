using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
namespace TouMarmo.Modules.Cosmetics;

public sealed record HeadDefinition(int Number, string ProductId, string AssetId)
{
    public string Name => Number switch { 1 => "Tusk", 2 => "Duda", 3 => "Friz", 4 => "Trampek", 5 => "Prince Polo", 6 => "Admiros", 7 => "Zonobijka", 8 => "Wojan", 9 => "Palion", _ => $"Head {Number:00}" };
    public string Resource => $"TouMarmo.Resources.HeadHats.Head{Number:00}.png";
}
public static class HeadHatCollection
{
    // Preserve the build 38 product IDs for existing saved outfits.
    public static readonly HeadDefinition[] Heads = {
        new(1,"4McdlMjYE_YfqNqoPY97coA8M1WeGp","235a4c04-1db8-b57d-1809-a2e32aa7aa00"),
        new(2,"i6VQOcurXtmARPByXriKI5Q6wbzvNi","6a47a4cc-2e36-ebfb-45f3-7628d3402625"),
        new(3,"BlvNAs7sq5u2_GbGosAuMfbqQshcrZ","9aae4e55-a113-42de-b4a5-2a1bb53d65f6"),
        new(4,"FO_HK0jrPtCvWtwr8OPgoTL3GlLGLQ","30fd45c8-17ab-c8fd-89ca-aac85750b078"),
        new(5,"4oddrL90xira26LctkXDav0-TlP4MO","e4a09acd-84ba-a67a-6600-a2a2502dadf7"),
        new(6,"mtou.hat.head06","859de3b8-ecf7-51ec-b373-caf55e26d2aa"),
        new(7,"mtou.hat.head07","c03bc4e0-8e7f-5f6b-97d3-ccfe9def6414"),
        new(8,"mtou.hat.head08","d1e65242-393c-572b-9605-4dcb8297d9ea"),
        new(9,"mtou.hat.head09","44f75190-724f-5ecc-84d2-ed0e5ea06d47") };
    private static bool registered;
    public static bool Contains(string id) => Heads.Any(h => h.ProductId == id);
    public static void Register()
    {
        if (registered) return;
        ClassInjector.RegisterTypeInIl2Cpp<MtouHatProvider>(new RegisterTypeOptions { Interfaces = new Il2CppInterfaceCollection(new[] { typeof(IResourceProvider) }) });
        ClassInjector.RegisterTypeInIl2Cpp<MtouHatLocator>(new RegisterTypeOptions { Interfaces = new Il2CppInterfaceCollection(new[] { typeof(IResourceLocator) }) });
        var provider = new MtouHatProvider();
        Addressables.ResourceManager.ResourceProviders.Insert(0, new IResourceProvider(provider.Pointer));
        var locator = new MtouHatLocator();
        Addressables.AddResourceLocator(new IResourceLocator(locator.Pointer));
        registered = true;
    }
    public static void Install(ReferenceDataManager manager)
    {
        foreach (var entry in Heads)
        {
            var existing = manager.Refdata.hats.ToArray().FirstOrDefault(h => h.ProductId == entry.ProductId);
            var added = !existing;
            var hat = added ? ScriptableObject.CreateInstance<HatData>() : existing!;
            hat.name = hat.StoreName = entry.Name; hat.ProductId = entry.ProductId; hat.Free = true;
            hat.BlocksVisors = true; hat.NoBounce = true; hat.InFront = true; hat.PreviewCrewmateColor = false;
            hat.ViewDataRef = new AssetReference(entry.AssetId, Il2CppType.Of<HatViewData>());
            hat.PreviewData = new AssetReference(entry.AssetId, Il2CppType.Of<PreviewViewData>());
            if (added) manager.Refdata.hats.Add(hat);
        }
    }
    public static Sprite LoadSprite(HeadDefinition head)
    {
        using var stream = typeof(HeadHatCollection).Assembly.GetManifestResourceStream(head.Resource)
            ?? throw new FileNotFoundException(head.Resource);
        var bytes = new byte[checked((int)stream.Length)];
        using (var reader = new BinaryReader(stream)) bytes = reader.ReadBytes(bytes.Length);
        var texture = new Texture2D(2,2);
        var opaquePng = bytes.Length > 25 && bytes[25] is 0 or 2;
        if (!texture.LoadImage(bytes, !opaquePng)) { UnityEngine.Object.Destroy(texture); throw new InvalidDataException(head.Resource); }
        if (opaquePng)
        {
            var pixels = texture.GetPixels32();
            var mask = HeadBackground.EdgeMask(texture.width, texture.height, i => pixels[i].r <= 3 && pixels[i].g <= 3 && pixels[i].b <= 3);
            for (var i = 0; i < mask.Length; i++) if (mask[i]) { var pixel = pixels[i]; pixel.a = 0; pixels[i] = pixel; }
            var alphaTexture = new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            alphaTexture.SetPixels32(pixels); alphaTexture.Apply(false,true);
            UnityEngine.Object.Destroy(texture); texture = alphaTexture;
        }
        texture.filterMode = FilterMode.Bilinear; texture.wrapMode = TextureWrapMode.Clamp;
        var sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(.5f,.5f), HeadHatLayout.PixelsPerUnit(texture.width,texture.height));
        sprite.name = head.Name; return sprite;
    }
    public static void ReleaseSprite(Sprite? sprite)
    {
        if (!sprite) return;
        if (sprite!.texture) UnityEngine.Object.Destroy(sprite.texture);
        UnityEngine.Object.Destroy(sprite);
    }
}
[HarmonyPatch(typeof(ReferenceDataManager),nameof(ReferenceDataManager.Initialize))]
public static class MtouInstallHeads
{
    [HarmonyPostfix,HarmonyPriority(Priority.Last)]
    public static void Postfix(ReferenceDataManager __instance, ref Il2CppSystem.Collections.IEnumerator __result) => __result = Install(__instance,__result).WrapToIl2Cpp();
    private static IEnumerator Install(ReferenceDataManager manager, Il2CppSystem.Collections.IEnumerator original)
    {
        while (original.MoveNext()) yield return original.Current;
        HeadHatCollection.Install(manager);
    }
}
