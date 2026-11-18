using UnityEngine;

namespace TouMarmo.Assets;

public static class ComposterAssets
{
    private const string ResourcePath = "TouMarmo.Resources.Composter";

    public static LoadableAsset<Sprite> RoleIcon { get; } =
        new LoadableResourceAsset($"{ResourcePath}.ComposterRoleIcon.png", 410f);

    public static LoadableAsset<Sprite> TrapButton { get; } =
        new LoadableResourceAsset($"{ResourcePath}.CompostTrapButton.png", 1004.18f);
    public static LoadableAsset<Sprite> TrapFloor { get; } =
        new LoadableResourceAsset($"{ResourcePath}.CompostTrapFloor.png", 1600f);
}

public static class BarszczAssets
{
    private const string ResourcePath = "TouMarmo.Resources.Barszcz";

    public static LoadableAsset<Sprite> RoleIcon { get; } =
        new LoadableResourceAsset($"{ResourcePath}.BarszczRoleIcon.png", 410f);

    public static LoadableAsset<Sprite> KillButton { get; } =
        new LoadableResourceAsset($"{ResourcePath}.BarszczKillButton.png", 410f);
}
