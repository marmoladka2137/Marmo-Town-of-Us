using UnityEngine;
namespace TouMarmo.Assets;
public static class FurasAssets
{
    public static LoadableAsset<Sprite> Pet { get; } = new LoadableResourceAsset("TouMarmo.Resources.Furas.Pet.png", 1000f);
    public static LoadableAsset<Sprite> Paw { get; } = new LoadableResourceAsset("TouMarmo.Resources.Furas.Paw.png", 1000f);
    public static LoadableAsset<Sprite> PlaceSpawn { get; } = new LoadableResourceAsset("TouMarmo.Resources.Furas.PlaceSpawn.png", 205f);
    public static LoadableAsset<Sprite> NextSpawn { get; } = new LoadableResourceAsset("TouMarmo.Resources.Furas.NextSpawn.png", 205f);
    public static LoadableAsset<Sprite> PetAttack { get; } = new LoadableResourceAsset("TouMarmo.Resources.Furas.PetAttack.png", 205f);
}
