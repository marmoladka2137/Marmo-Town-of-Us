using HarmonyLib;
using InnerNet;

namespace TouMarmo.Patches;

// Matchmaking registration must follow the connection mode. The built-in LAN
// server cannot process online registration data while accepting a local join.
[HarmonyPatch]
internal static class LocalModRegistrationPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    private static void Prefix(InnerNetClient __instance)
    {
        CurrentModRegistration.ModRegistrationGuidString =
            __instance.NetworkMode == NetworkModes.OnlineGame
                ? TouMarmoPlugin.LobbyRegistrationGuid
                : string.Empty;
    }
}
