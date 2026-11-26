using TMPro;
using UnityEngine;
namespace TouMarmo.Patches;

// Only styles the numeric text; never changes its contents, visibility or timer.
public static class CooldownVisualStyle
{
    public static void Apply(TextMeshPro? text, TextMeshPro sample, float size, Vector3 worldScale)
    {
        if (!text || !sample || !float.IsFinite(size) || size <= 0) return;
        var target = text!;
        target.enableAutoSizing = false;
        target.fontSize = size;
        target.fontSizeMin = size;
        target.fontSizeMax = size;
        if (sample.font && target.font != sample.font) target.font = sample.font;
        target.fontStyle = sample.fontStyle;
        target.fontWeight = sample.fontWeight;
        target.enableWordWrapping = false;
        target.overflowMode = TextOverflowModes.Overflow;
        // Equal local scales do not guarantee equal rendered sizes when buttons
        // have differently scaled parents. Match the reference's world scale.
        var parent = target.transform.parent;
        var inherited = parent ? parent!.lossyScale : Vector3.one;
        if (Mathf.Abs(inherited.x) > .001f && Mathf.Abs(inherited.y) > .001f &&
            Mathf.Abs(worldScale.x) > .001f && Mathf.Abs(worldScale.y) > .001f)
            target.transform.localScale = new Vector3(worldScale.x / inherited.x,
                worldScale.y / inherited.y, Mathf.Abs(inherited.z) > .001f ? worldScale.z / inherited.z : 1f);
    }
}
