using HarmonyLib;
using TMPro;
using UnityEngine;
namespace TouMarmo.Modules;
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class MarmoIntroLayout
{
    [HarmonyPostfix] public static void Postfix()
    {
        var intro = IntroCutscene.Instance;
        if (!intro) return;
        var text = intro.RoleBlurbText;
        if (!text) return;
        text!.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSizeMin = 1.2f;
        text.fontSizeMax = 3f;
        text.rectTransform.sizeDelta = new Vector2(9f,2f);
        text.overflowMode = TextOverflowModes.Ellipsis;
    }
}

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class MarmoRolePanelLayout
{
    [HarmonyPrefix] public static void Prefix(TaskPanelBehaviour __instance, string __0)
    {
        if (string.IsNullOrEmpty(__0) || !__0.Contains("Alignment")) return;
        var text = __instance.taskText;
        if (!text) return;
        text.enableWordWrapping = true;
        var size = text.rectTransform.sizeDelta;
        text.rectTransform.sizeDelta = new Vector2(4.8f, size.y);
        text.fontSize = Mathf.Min(text.fontSize, 1.7f);
    }

    [HarmonyPostfix] public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (__instance.gameObject.name == "RolePanel") AlignText(__instance);
    }

    internal static void AlignText(TaskPanelBehaviour panel)
    {
        var text=panel.taskText;
        if (!text || string.IsNullOrEmpty(text.text)) return;
        text.ForceMeshUpdate();
        var bounds=text.textBounds;
        var upperLeft=panel.transform.InverseTransformPoint(text.transform.TransformPoint(
            new Vector3(bounds.min.x,bounds.max.y,bounds.center.z)));
        var correction=panel.transform.TransformVector(new Vector3(0.2f-upperLeft.x,-0.15f-upperLeft.y,0));
        text.transform.position+=correction;
        text.ForceMeshUpdate();
    }
}

[HarmonyPatch(typeof(TaskPanelBehaviour), "Update")]
public static class MarmoPanelScreenBounds
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        var panel = TownOfUs.Events.TownOfUsEventHandlers.RolePanel;
        var camera = HudManager.Instance ? HudManager.Instance.UICamera : null;
        if (!panel || panel != __instance || !panel.open || camera == null || !panel.taskText) return;
        MarmoRolePanelLayout.AlignText(panel);
        panel.taskText.ForceMeshUpdate();
        var renderer=panel.taskText.GetComponent<MeshRenderer>();
        if (!renderer) return;
        var bounds=renderer!.bounds;
        foreach(var background in panel.GetComponentsInChildren<SpriteRenderer>())
            if(background && background.enabled) bounds.Encapsulate(background.bounds);
        var bottom = camera.ScreenToWorldPoint(new Vector3(0,0, camera.nearClipPlane));
        var top = camera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,camera.nearClipPlane));
        float x = 0, y = 0;
        if (bounds.min.x < bottom.x + 0.2f) x = bottom.x + 0.2f - bounds.min.x;
        if (bounds.max.x + x > top.x - 0.45f) x += top.x - 0.45f - (bounds.max.x + x);
        if (bounds.min.y < bottom.y + 0.25f) y = bottom.y + 0.25f - bounds.min.y;
        if (bounds.max.y + y > top.y - 0.25f) y += top.y - 0.25f - (bounds.max.y + y);
        var correction=new Vector3(x,y,0);
        panel.transform.position += correction;
        var parent=panel.transform.parent;
        panel.openPosition += parent ? parent.InverseTransformVector(correction) : correction;
    }
}
