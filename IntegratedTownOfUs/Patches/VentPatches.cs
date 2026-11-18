// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Modules.Plumber;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class VentPatches
{
    public static bool InVision(PlayerControl player)
    {
        return InVision(player, PlayerControl.LocalPlayer);
    }

    public static bool InVision(PlayerControl player, PlayerControl observer)
    {
        var observerPosition = observer.GetTruePosition();
        var vector = player.GetTruePosition() - observerPosition;
        var magnitude = vector.magnitude;

        if (magnitude < PlayerControl.LocalPlayer.lightSource.viewDistance &&
            !PhysicsHelpers.AnyNonTriggersBetween(observerPosition, vector.normalized, magnitude,
                Constants.ShipAndObjectsMask))
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    [HarmonyPrefix]
    public static bool EnterVentPatch(Vent __instance, PlayerControl pc)
    {
        // Host handles local and remote entries, even when vent animation is hidden.
        if (pc != null && MarmoSession.IsHost && PlumberNailRuntime.IsTrapped(__instance.Id))
        {
            PlumberNailRuntime.RpcVentEntry(pc, __instance.Id);
        }

        if (!__instance.EnterVentAnim)
        {
            return true;
        }

        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.HideVentAnimationNotInVision.Value)
        {
            return true;
        }

        if (InVision(pc))
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    [HarmonyPrefix]
    public static bool ExitVentPatch(Vent __instance, PlayerControl pc)
    {
        // Clear entry processed flag when exiting vent
        if (pc) PlumberNailRuntime.OnVentExit(pc, __instance.Id);
        
        if (!__instance.ExitVentAnim)
        {
            return true;
        }

        if (!OptionGroupSingleton<VanillaTweakOptions>.Instance.HideVentAnimationNotInVision.Value)
        {
            return true;
        }

        if (InVision(pc))
        {
            return true;
        }

        return false;
    }
}
