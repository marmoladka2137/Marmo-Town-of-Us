using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(LoadingBarManager), nameof(LoadingBarManager.ToggleLoadingBar))]
public static class LoadingPatch
{
    public static void Postfix(LoadingBarManager __instance)
    {
        var logo = __instance.loadingBar.transform.GetChild(1).GetChild(0);
        var image = logo.GetComponent<Image>();
        image.sprite = TouAssets.BannerDark.LoadAsset();
        image.preserveAspect = true;
        logo.localScale = Vector3.one;
    }
}
