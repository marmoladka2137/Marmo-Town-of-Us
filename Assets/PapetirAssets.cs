using UnityEngine;
namespace TouMarmo.Assets;
public static class PapetirAssets
{
    public static LoadableAsset<Sprite> Paper { get; }=new LoadableResourceAsset("TouMarmo.Resources.Icons.Papetir.png",1000f);
}
