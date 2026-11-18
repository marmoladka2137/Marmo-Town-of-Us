// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public static class ModStampPatch
{
    public static ModStampLocation StampPlacement = ModStampLocation.TopLeft;
    [HarmonyPostfix]
    public static void LateUpdate(ModManager __instance)
    {
        // MTOU release preparation, 2026-11-17: keep the required stamp active.
        if (!__instance.ModStamp) return;
        if (!__instance.ModStamp.gameObject.activeSelf) __instance.ShowModStamp();
        if (!__instance.localCamera)
        {
            if (HudManager.InstanceExists)
            {
                __instance.localCamera = HudManager.Instance.GetComponentInChildren<Camera>();
            }
            else
            {
                __instance.localCamera = Camera.main;
            }
        }

        if (__instance.localCamera)
        {
            var position = StampPlacement switch
            {
                ModStampLocation.TopLeft => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftTop,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                ModStampLocation.BottomLeft => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.LeftBottom,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                ModStampLocation.BottomRight => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightBottom,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f)),
                _ => AspectPosition.ComputeWorldPosition(__instance.localCamera, AspectPosition.EdgeAlignments.RightTop,
                    new Vector3(0.6f, 0.6f, __instance.localCamera!.nearClipPlane + 0.1f))
            };
            __instance.ModStamp.transform.position = position;
        }

    }
}
