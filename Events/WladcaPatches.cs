using HarmonyLib;
using MiraAPI.GameOptions;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Options.Maps;
using UnityEngine;

namespace TouMarmo.Events;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class WladcaLightPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (player?.Object?.Data?.Role is not WladcaRole { Empowered: true } ||
            !OptionGroupSingleton<WladcaOptions>.Instance.SeeInLights.Value)
        {
            return;
        }

        var fullCrewVision = __instance.MaxLightRadius *
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod *
            TownOfUsMapOptions.GetMapBasedCrewmateVision();
        __result = Mathf.Max(__result, fullCrewVision);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AreCommsAffected))]
public static class WladcaCommsPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, ref bool __result)
    {
        if (__instance.Data?.Role is WladcaRole { Empowered: true } &&
            OptionGroupSingleton<WladcaOptions>.Instance.IgnoreComms.Value)
        {
            __result = false;
        }
    }
}
