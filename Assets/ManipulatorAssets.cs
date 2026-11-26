using UnityEngine;
namespace TouMarmo.Assets;
public static class ManipulatorAssets
{
    public static LoadableAsset<Sprite> Role { get; } = new LoadableResourceAsset("TouMarmo.Resources.Manipulator.Role.png", 1000f);
    public static LoadableAsset<Sprite> Tablet { get; } = new LoadableResourceAsset("TouMarmo.Resources.Manipulator.Tablet.png", 1000f);
}
