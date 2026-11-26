namespace TouMarmo.Modules.Cosmetics;

public static class HeadHatLayout
{
    // Native Corsac hats use world-space sprites. Normalize the supplied large PNGs
    // without rewriting the artwork or using its original size as in-game units.
    public const float LongestSide = 2.45f;
    public static float PixelsPerUnit(int width, int height) =>
        Math.Max(1, Math.Max(width, height)) / LongestSide;
}
