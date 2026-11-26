using UnityEngine;
namespace TouMarmo.Assets;
public static class GravediggerAssets
{
    public static LoadableAsset<Sprite> Role { get; } = new LoadableResourceAsset("TouMarmo.Resources.Gravedigger.Role.png",1000f);
    public static LoadableAsset<Sprite> Bury { get; } = new LoadableResourceAsset("TouMarmo.Resources.Gravedigger.Bury.png",1000f);
    public static LoadableAsset<Sprite> Inherit { get; } = new LoadableResourceAsset("TouMarmo.Resources.Gravedigger.Inherit.png",1000f);
}
