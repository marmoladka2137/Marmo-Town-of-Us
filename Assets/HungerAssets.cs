using UnityEngine;
namespace TouMarmo.Assets;

public static class HungerAssets
{
    public static LoadableAsset<Sprite> Icon { get; } = new LoadableResourceAsset("TouMarmo.Resources.Icons.Hunger.png", 1000f);
}
