using UnityEngine;

namespace TouMarmo.Assets;

public static class HuggerAssets
{
    public static LoadableAsset<Sprite> Hug { get; } =
        new LoadableResourceAsset("TouMarmo.Resources.Hugger.Hugger.png", 410f);
}
