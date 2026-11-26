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
        if (!panel || panel != __instance || camera == null || !panel.taskText) return;
        // A role tab is a flat HUD element. Cancel the cloned task panel's tilt.
        panel.transform.rotation = camera.transform.rotation;
        panel.background.transform.localRotation = Quaternion.identity;
        panel.tab.transform.localRotation = Quaternion.identity;
        panel.taskText.transform.localRotation = Quaternion.identity;
        // Mira interpolates both axes, but the role drawer must only slide sideways.
        // Apply this in both states, not just while open, so toggling never changes Y.
        var anchorY = Mathf.Min(panel.closedPosition.y, panel.openPosition.y);
        panel.closedPosition = new Vector3(panel.closedPosition.x,anchorY,panel.closedPosition.z);
        panel.openPosition = new Vector3(panel.openPosition.x,anchorY,panel.openPosition.z);
        panel.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop,
            new Vector3(Mathf.SmoothStep(panel.closedPosition.x,panel.openPosition.x,panel.timer),anchorY,panel.openPosition.z));
        MarmoRolePanelLayout.AlignText(panel);
        panel.taskText.ForceMeshUpdate();
        var renderer=panel.taskText.GetComponent<MeshRenderer>();
        if (!renderer) return;
        var bounds=renderer!.bounds;
        foreach(var background in panel.GetComponentsInChildren<SpriteRenderer>())
            if(background && background.enabled) bounds.Encapsulate(background.bounds);
        var depth=Mathf.Max(camera.nearClipPlane,Vector3.Dot(panel.transform.position-camera.transform.position,camera.transform.forward));
        var bottom = camera.ScreenToWorldPoint(new Vector3(0,0,depth));
        var top = camera.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,depth));
        if (!panel.open) return;
        float x = 0;
        if (bounds.min.x < bottom.x + 0.2f) x = bottom.x + 0.2f - bounds.min.x;
        if (bounds.max.x + x > top.x - 0.45f) x += top.x - 0.45f - (bounds.max.x + x);
        // Never push the entire drawer upward to fit its contents. Size the text
        // area instead, keeping the tab anchored alongside the task panel.
        var text=panel.taskText;
        var available=Mathf.Max(.6f,(panel.transform.position.y-bottom.y-.25f)/Mathf.Max(.01f,Mathf.Abs(panel.transform.lossyScale.y)));
        text.enableAutoSizing=true;
        text.fontSizeMax=1.7f;
        text.fontSizeMin=.8f;
        text.enableWordWrapping=true;
        text.overflowMode=TextOverflowModes.Ellipsis;
        text.rectTransform.sizeDelta=new Vector2(4.8f,available);
        var correction=new Vector3(x,0,0);
        if(panel.timer >= .999f) panel.transform.position += correction;
    }
}
