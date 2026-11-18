using UnityEngine;

namespace TownOfUs.Assets;

public static class ComposterAssets
{
    private const string ResourcePath = "TownOfUs.Resources.Composter";

    public static LoadableAsset<Sprite> RoleIcon { get; } =
        new LoadableResourceAsset($"{ResourcePath}.ComposterRoleIcon.png", 220f);

    public static LoadableAsset<Sprite> TrapButton { get; } =
        new LoadableResourceAsset($"{ResourcePath}.CompostTrapButton.png", 220f);
}
