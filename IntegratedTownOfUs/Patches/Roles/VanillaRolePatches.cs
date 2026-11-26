using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(RoleBehaviour))]
public static class VanillaRolePatches
{
    [HarmonyPatch(nameof(RoleBehaviour.TeamColor), MethodType.Getter)]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CrewmateColorPrefix(RoleBehaviour __instance, ref Color __result)
    {
        if (!__instance.IsCrewmate())
        {
            return true;
        }

        if (TownOfUsColors.UseBasic)
        {
            __result = Palette.CrewmateBlue;
            return false;
        }

        if (!__instance.IsCustomRole())
        {
            var newColor = __instance.Role switch
            {
                RoleTypes.GuardianAngel => TownOfUsColors.GuardianAngel,
                RoleTypes.Detective => TownOfUsColors.Detective,
                RoleTypes.Tracker => TownOfUsColors.Tracker,
                RoleTypes.Scientist => TownOfUsColors.Scientist,
                RoleTypes.Noisemaker => TownOfUsColors.Noisemaker,
                RoleTypes.Engineer => TownOfUsColors.Engineer,
                RoleTypes.Judge => TownOfUsColors.Prosecutor,
                RoleTypes.SpiritGuide => TownOfUsColors.Spectator,
                _ => Palette.CrewmateBlue
            };
            __result = newColor;
            return false;
        }

        return true;
    }

}

// RoleIconSolid is a native IL2CPP field in the supported game, not a native
// method. Detouring its generated CLR accessor falls back to the managed patch
// backend and can corrupt method resolution during startup. Set the same icon
// on the returned role instead of detouring that field accessor.
[HarmonyPatch(typeof(RoleManager),nameof(RoleManager.GetRole))]
public static class VanillaRoleIcons
{
    [HarmonyPostfix]
    public static void Postfix(RoleBehaviour __result)
    {
        if (!__result || __result.IsCustomRole())
        {
            return;
        }

        var newIcon = TouRoleUtils.TryGetVanillaRoleIcon(__result.Role);
        if (newIcon != null)
        {
            __result.RoleIconSolid = newIcon;
        }
    }
}
