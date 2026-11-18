// Original MTOU replacement, 2026-11-18, GPL-3.0. Historical Maxi0fc feature credit remains in CREDITS.md.
// Keeps the public integration names and host preferences; no external AutoRejoin implementation is copied here.
using HarmonyLib;
using TownOfUs.Modules.AutoRejoin;
using TouMarmo.Modules.OriginalMotion;

namespace TownOfUs.Patches.Features;

[HarmonyPatch]
public static class AutoRejoinPatches
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start)), HarmonyPostfix]
    public static void EndGameStartPostfix(EndGameManager __instance)
    {
        var client = AmongUsClient.Instance;
        if (!client || TutorialManager.InstanceExists) return;
        int mode = (int)LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.AutoRejoinMode.Value;
        if (!RejoinSchedule.Eligible(mode, client.AmHost)) return;
        RejoinBehaviour.CurrentEndGameManager = __instance;
        RejoinBehaviour.SavedGameId = client.GameId;
        RejoinBehaviour.TriggerRejoin();
    }
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start)), HarmonyPostfix]
    public static void LobbyStartPostfix() => RejoinBehaviour.CancelRejoin();
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame)), HarmonyPrefix]
    public static void ExitGamePrefix() => RejoinBehaviour.CancelRejoin();
}
