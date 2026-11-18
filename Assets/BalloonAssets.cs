using UnityEngine;

namespace TouMarmo.Assets;

public static class BalloonAssets
{
    public static LoadableAsset<Sprite> Head { get; } = new LoadableResourceAsset("TouMarmo.Resources.Balloon.Balloon.png", 330f);
    public static LoadableAsset<Sprite> Icon { get; } =
        new LoadableResourceAsset("TouMarmo.Resources.Balloon.Balloon.png", 410f);
}
