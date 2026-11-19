using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(LoadingBarManager), nameof(LoadingBarManager.ToggleLoadingBar))]
public static class LoadingPatch
{
    public static void Postfix(LoadingBarManager __instance)
    {
        if (!__instance || !__instance.loadingBar || __instance.loadingBar.transform.childCount < 2) return;
        var holder = __instance.loadingBar.transform.GetChild(1);
        if (holder.childCount == 0) return;
        var logo = holder.GetChild(0);
        var image = logo.GetComponent<Image>();
        if (!image) return;
        image.sprite = TouAssets.BannerDark.LoadAsset();
        image.preserveAspect = true;
        logo.localScale = Vector3.one;
    }
}
