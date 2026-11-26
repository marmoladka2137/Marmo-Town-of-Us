using HarmonyLib;
using MiraAPI.Hud;
using UnityEngine;
namespace TouMarmo.Patches;

// Presentation only: never write Timer, Cooldown, UsesLeft or effect duration.
[HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
public static class AbilityVisualSizes
{
    private static ActionButton? typographyReference;
    private static float cooldownFontSize, labelFontSize;
    private static Vector3 cooldownLocalScale, labelLocalScale;
    public const float CooldownTextScale = 1.30f;
    // MiraAPI clones HudManager.AbilityButton without changing its typography.
    // Capture before TMP can shrink the inactive custom Fix button's digits.
    public static void CaptureNative(HudManager hud)
    {
        var reference=hud.AbilityButton;
        if(!reference || !reference.cooldownTimerText || !reference.buttonLabelText) return;
        var sample=reference.cooldownTimerText;
        cooldownFontSize=sample.fontSize;
        cooldownLocalScale=sample.transform.localScale;
        labelFontSize=reference.buttonLabelText.fontSize;
        labelLocalScale=reference.buttonLabelText.transform.localScale;
        typographyReference=reference;
    }
    // Fallback only; live HUD fitting uses the unmodified native Report graphic.
    public const float MiraAbilityDiameter = 1.10f;
    private static readonly Dictionary<int, float> VisibleExtents = new();

    public static int VisiblePixelExtent(Color32[] pixels, int width, int height)
    {
        if (width <= 0 || height <= 0 || pixels.Length != (long)width * height) return 0;
        var left = width; var right = -1; var bottom = height; var top = -1;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            if (pixels[y * width + x].a <= 32) continue;
            left = Math.Min(left, x); right = Math.Max(right, x);
            bottom = Math.Min(bottom, y); top = Math.Max(top, y);
        }
        return right < left ? 0 : Math.Max(right - left + 1, top - bottom + 1);
    }

    public static void FitGraphic(SpriteRenderer? graphic,float worldDiameter=MiraAbilityDiameter)
    {
        if (!graphic || !graphic!.sprite) return;
        var sprite = graphic.sprite;
        if (!sprite.texture || !float.IsFinite(worldDiameter) || worldDiameter<=0) return;
        var id = sprite.GetInstanceID();
        if (!VisibleExtents.TryGetValue(id, out var extent))
        {
            extent = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            // A native atlas contains other icons too; its mesh bounds provide
            // the sprite's extent. Read only MTOU's standalone resource textures.
            if (sprite.texture.isReadable && sprite.texture.name.StartsWith("TouMarmo.Resources.", StringComparison.Ordinal))
            {
                var pixels = sprite.texture.GetPixels32().ToArray();
                var visible = VisiblePixelExtent(pixels, sprite.texture.width, sprite.texture.height);
                if (visible > 0 && sprite.pixelsPerUnit > 0) extent = visible / sprite.pixelsPerUnit;
            }
            VisibleExtents[id] = extent;
        }
        if (extent <= .001f) return;
        graphic.drawMode = SpriteDrawMode.Simple;
        var parent=graphic.transform.parent;
        var inherited=parent ? parent!.lossyScale : Vector3.one;
        if(Mathf.Abs(inherited.x)<=.001f || Mathf.Abs(inherited.y)<=.001f) return;
        var scale=worldDiameter/extent;
        graphic.transform.localScale=new Vector3(scale/Mathf.Abs(inherited.x),scale/Mathf.Abs(inherited.y),1f);
    }
    public static bool IsPracticeTool(Type type) => type.Namespace=="TownOfUs.Buttons.BaseFreeplay";
    public static IEnumerable<ActionButton> SelectButtons(IEnumerable<CustomActionButton> custom,IEnumerable<ActionButton> native)
    {
        var all=custom.ToArray();
        var tools=all.Where(a=>IsPracticeTool(a.GetType()) && a.Button!=null).Select(a=>a.Button!).ToHashSet();
        return all.Select(a=>a.Button).Where(b=>b!=null).Select(b=>b!).Concat(native).Distinct().Where(b=>!tools.Contains(b));
    }
    [HarmonyPostfix,HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        var hud=HudManager.Instance;
        if(!hud) return;
        var reference=hud!.AbilityButton;
        if (reference==null || !reference || !reference.cooldownTimerText || !reference.buttonLabelText) return;
        var sample=reference.cooldownTimerText;
        var label=reference.buttonLabelText;
        if (typographyReference != reference) CaptureNative(hud);
        var worldScale=Vector3.Scale(cooldownLocalScale, sample.transform.parent ? sample.transform.parent.lossyScale : Vector3.one);
        var labelWorldScale=Vector3.Scale(labelLocalScale, label.transform.parent ? label.transform.parent.lossyScale : Vector3.one);
        var native=hud ? hud.GetComponentsInChildren<ActionButton>(true).ToArray() : Array.Empty<ActionButton>();
        // Report is never fitted, so it is a stable original-Mira-sized anchor
        // even when Mira's UI-size setting changes the button parents.
        var report=hud.ReportButton ? hud.ReportButton.graphic : null;
        var diameter=report && report!.sprite ? Mathf.Max(
            report.sprite.bounds.size.x*Mathf.Abs(report.transform.lossyScale.x),
            report.sprite.bounds.size.y*Mathf.Abs(report.transform.lossyScale.y)) : MiraAbilityDiameter;
        if (MeetingHud.Instance)
            native = native.Concat(MeetingHud.Instance.GetComponentsInChildren<ActionButton>(true)).ToArray();
        foreach(var b in SelectButtons(CustomButtonManager.Buttons,native))
        {
            if(b==null || !b) continue;
            if (b.graphic && b.graphic!=report && b.graphic.transform != b.transform) FitGraphic(b.graphic,diameter);
            CooldownVisualStyle.Apply(b.cooldownTimerText, sample, cooldownFontSize * CooldownTextScale, worldScale);
            if (b.graphic && b.graphic.sprite && b.graphic.sprite.texture &&
                b.graphic.sprite.texture.name.StartsWith("TouMarmo.Resources.", StringComparison.Ordinal) && b.buttonLabelText)
            {
                CooldownVisualStyle.Apply(b.buttonLabelText, label, labelFontSize, labelWorldScale);
                // Only labels may shrink for long names; digits always stay fixed.
                b.buttonLabelText.enableAutoSizing=true;
                b.buttonLabelText.fontSizeMin=labelFontSize*.85f;
            }
        }
    }
}

[HarmonyPatch(typeof(HudManager),nameof(HudManager.Start))]
public static class NativeAbilityTypographyCapture
{
    [HarmonyPostfix,HarmonyPriority(Priority.First)]
    public static void Postfix(HudManager __instance)=>AbilityVisualSizes.CaptureNative(__instance);
}
