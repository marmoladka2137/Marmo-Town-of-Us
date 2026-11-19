using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

// Cosmetic replacement only. The game owns the startup animation and scene transition.
[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
internal static class SplashIntroPatch
{
    [HarmonyPostfix]
    private static void Postfix(SplashManager __instance)
    {
        if (!__instance || !__instance.logoAnimFinish) return;
        var logo = __instance.logoAnimFinish.transform.Find("LogoRoot/PEWLogo");
        if (!logo) return;
        var image = logo.GetComponent<SpriteRenderer>();
        if (image) image.sprite = TouAssets.AuAvengersLogo.LoadAsset();
    }
}
