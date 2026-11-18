using UnityEngine;

namespace TouMarmo.Assets;

/// <summary>One supplied logo for every local MTOU branding surface.</summary>
public static class MarmoBranding
{
    public const string ResourcePath = "TouMarmo.Resources.Branding.MarmoLogo.png";
    // Display bounds only: the owner's original PNG is embedded byte-for-byte.
    // Four pixels of padding around visible alpha >= 16 exclude transparent margins/noise.
    public static readonly Rect ContentRect = new(33, 258, 1193, 679);
    public static LoadableAsset<Sprite> Banner { get; } = new LogoAsset(100f);
    public static LoadableAsset<Sprite> Splash { get; } = new LogoAsset(170f);
    public static LoadableAsset<Sprite> Icon { get; } = new LogoAsset(700f);
    private static readonly LoadableResourceAsset Original = new(ResourcePath);

    private sealed class LogoAsset(float pixelsPerUnit) : LoadableAsset<Sprite>
    {
        public override Sprite LoadAsset()
        {
            if (!LoadedAsset)
            {
                var texture = Original.LoadAsset().texture;
                LoadedAsset = Sprite.Create(texture, ContentRect, new Vector2(.5f, .5f), pixelsPerUnit);
                LoadedAsset.name = "Marmo Town of Us logo";
            }
            return LoadedAsset!;
        }
    }
}
