using HarmonyLib;
using MiraAPI.Hud;

namespace TouMarmo.Patches;

[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.SetUses))]
public static class UnlimitedUseBadgePatch
{
    [HarmonyPostfix]
    public static void Postfix(CustomActionButton __instance)
    {
        if (__instance.ZeroIsInfinite && __instance.MaxUses == 0 && __instance.Button)
            __instance.Button!.SetInfiniteUses();
    }
}
