using HarmonyLib;
using TownOfUs.Patches;
using UnityEngine;
namespace TouMarmo.Modules.DraftSafety;
public static class DraftUiState
{
    private static ChatController? chat;
    private static Vector3 scale,screenScale;
    private static float targetScale;
    public static void Capture()
    {
        if(chat || !HudManager.Instance || !HudManager.Instance.Chat) return;
        chat=HudManager.Instance.Chat;scale=chat.transform.localScale;
        screenScale=chat.chatScreen.transform.localScale;targetScale=chat.targetScale;
    }
    public static void Restore()
    {
        if(!chat){chat=null;return;}
        HudManagerPatches.ResetZoom();
        if(chat){chat!.transform.localScale=scale;if(chat.chatScreen) chat.chatScreen.transform.localScale=screenScale;chat.targetScale=targetScale;}
        chat=null;
    }
}
[HarmonyPatch(typeof(ShipStatus),nameof(ShipStatus.Start))]
public static class DraftGameplayUiPatch
{
    [HarmonyPostfix] public static void Postfix()=>DraftUiState.Restore();
}
