using HarmonyLib;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TouMarmo.Modules;

/// <summary>Fit the tall two-line logo above Play, below the account strip.</summary>
[RegisterInIl2Cpp]
public sealed class MenuBrandingLayout(IntPtr ptr) : MonoBehaviour(ptr)
{
    private MainMenuManager? menu;
    private float nextSweep;
    public void Initialize(MainMenuManager owner) => menu = owner;

    private void LateUpdate()
    {
        if (!menu) return;
        if (Time.unscaledTime >= nextSweep)
        {
            nextSweep = Time.unscaledTime + .25f;
            HideAchievementButtons(menu!.transform);
            MenuCommunityLink.Ensure(menu!);
        }
        Fit(menu!);
        MenuCommunityLink.Position(menu!);
        TouMarmo.Modules.Profile.CosmeticSelectionRuntime.Tick();
    }

    internal static Rect AvailableSlot(Rect play, Rect safe)
    {
        var gap = Mathf.Max(8f, safe.height * .009f);
        var left = Mathf.Max(safe.xMin + gap, play.xMin + gap);
        var right = Mathf.Min(safe.xMax - gap, play.xMax - gap);
        var bottom = play.yMax + gap;
        // Account/friends strip occupies the top of the screen, independently of logo height.
        var top = safe.yMax - safe.height * .103f - gap;
        return right > left && top > bottom ? Rect.MinMaxRect(left,bottom,right,top) : Rect.zero;
    }

    internal static bool TryPlayRect(MainMenuManager owner, Camera camera, out Rect rect)
    {
        rect = Rect.zero;
        if (!owner.playButton || !owner.playButton.gameObject.activeInHierarchy) return false;
        var parts = owner.playButton.GetComponentsInChildren<SpriteRenderer>(true)
            .Where(r => r && r.enabled && r.gameObject.activeInHierarchy && r.bounds.size.x > .05f && r.bounds.size.y > .05f).ToArray();
        if (parts.Length == 0) return false;
        var bounds = parts[0].bounds;
        foreach (var part in parts.Skip(1)) bounds.Encapsulate(part.bounds);
        var min = camera.WorldToScreenPoint(bounds.min);
        var max = camera.WorldToScreenPoint(bounds.max);
        rect = Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        return rect.width > 0 && rect.height > 0;
    }

    internal static void Fit(MainMenuManager owner)
    {
        var camera = Camera.main;
        var logo = TownOfUs.Patches.Misc.LogoPatch.GameLogo;
        if (!camera || !logo || !logo.sprite || !TryPlayRect(owner,camera,out var play)) return;
        var safe = Screen.safeArea;
        var viewport = camera.pixelRect;
        safe = Rect.MinMaxRect(Mathf.Max(safe.xMin,viewport.xMin),Mathf.Max(safe.yMin,viewport.yMin),
            Mathf.Min(safe.xMax,viewport.xMax),Mathf.Min(safe.yMax,viewport.yMax));
        var slot = AvailableSlot(play,safe);
        if (slot.height < 4 || slot.width < 4) { logo.enabled = false; return; }
        logo.enabled = true;
        var aspect = logo.sprite.rect.width / logo.sprite.rect.height;
        var height = Mathf.Min(slot.height,slot.width/aspect);
        var width = height*aspect;
        var min = camera.WorldToScreenPoint(logo.bounds.min);
        var max = camera.WorldToScreenPoint(logo.bounds.max);
        if (max.x-min.x < .01f || max.y-min.y < .01f) return;
        var scale = logo.transform.localScale;
        logo.transform.localScale = new Vector3(scale.x*width/(max.x-min.x),scale.y*height/(max.y-min.y),scale.z);
        var center = camera.WorldToScreenPoint(logo.bounds.center);
        var target = camera.ScreenToWorldPoint(new Vector3(slot.center.x,slot.center.y,center.z));
        logo.transform.position += target-logo.bounds.center;
    }

    internal static void HideAchievementButtons(Transform root)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
            if (child.name is "AchievementsMenuButton" or "Achievements Button")
                child.gameObject.SetActive(false);
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuBrandingSetup
{
    [HarmonyPostfix]
    public static void Postfix(MainMenuManager __instance)
    {
        var sizer = GameObject.Find("Sizer")?.GetComponent<AspectSize>();
        if (sizer) sizer!.enabled = false;
        var layout = __instance.GetComponent<MenuBrandingLayout>() ?? __instance.gameObject.AddComponent<MenuBrandingLayout>();
        layout.Initialize(__instance);
        MenuBrandingLayout.HideAchievementButtons(__instance.transform);
        MenuCommunityLink.Ensure(__instance);
    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Update))]
public static class HideAchievementsOptionsEntry
{
    [HarmonyPostfix]
    public static void Postfix(OptionsMenuBehaviour __instance) => MenuBrandingLayout.HideAchievementButtons(__instance.transform);
}
