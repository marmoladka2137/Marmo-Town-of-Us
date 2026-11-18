using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TouMarmo.Modules;

public static class MenuCommunityLink
{
    public const string Label = "dc.gg/mtou";
    public const string Url = "https://dc.gg/mtou";
    public const string ObjectName = "MarmoCommunityLink";

    public static void Ensure(MainMenuManager menu)
    {
        var parent = menu.transform.Find("MainUI/AspectScaler") ?? menu.transform;
        var visible=!LobbyBehaviour.Instance && menu.playButton && menu.playButton.gameObject.activeInHierarchy;
        var existing=parent.Find(ObjectName);
        if(existing){existing.gameObject.SetActive(visible);return;}
        if(!visible) return;
        var fallback=menu.newsButton ? menu.newsButton.GetComponentsInChildren<TextMeshPro>(true).FirstOrDefault()?.font : null;
        Create(parent,fallback);
        Position(menu);
    }

    public static void EnsureLobby(HudManager hud)
    {
        var existing=hud.transform.Find(ObjectName);
        var visible=LobbyBehaviour.Instance && !hud.Chat.IsOpenOrOpening &&
            !(GameSettingMenu.Instance && GameSettingMenu.Instance.gameObject.activeInHierarchy) &&
            !(PlayerCustomizationMenu.Instance && PlayerCustomizationMenu.Instance.gameObject.activeInHierarchy);
        if(existing) existing.gameObject.SetActive(visible);
        if(!visible) return;
        if(!existing) existing=Create(hud.transform,null).transform;
        PositionRoot(existing,.27f);
    }

    private static GameObject Create(Transform parent,TMP_FontAsset? fallback)
    {
        var root = new GameObject(ObjectName);
        root.layer = LayerMask.NameToLayer("UI");
        root.transform.SetParent(parent,false);
        root.transform.localPosition = new Vector3(0,0,-5f);

        var text = root.AddComponent<TextMeshPro>();
        var font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "LiberationSans SDF")
            ?? TMP_Settings.defaultFontAsset ?? fallback;
        if (font)
        {
            text.font = font;
            text.fontSharedMaterial = font!.material;
        }
        text.text = Label;
        text.fontSize = text.fontSizeMin = text.fontSizeMax = 3f;
        text.enableAutoSizing = false;
        text.fontStyle = FontStyles.Underline;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.color = Color.white;
        text.rectTransform.sizeDelta = new Vector2(2.8f,.5f);
        text.GetComponent<MeshRenderer>().sortingOrder = 20;

        var collider = root.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(2.8f,.5f);
        collider.isTrigger = true;
        var button = root.AddComponent<PassiveButton>();
        button.ClickMask = collider;
        button.Colliders = new Il2CppReferenceArray<Collider2D>(new Collider2D[]{collider});
        button.activeSprites = Child(root.transform,"Active");
        button.inactiveSprites = Child(root.transform,"Inactive");
        button.buttonText = text;
        // Native PassiveButton applies these every state transition. Its default
        // colors are transparent on a component created without a prefab.
        var hover = new Color32(70,225,205,255);
        button.activeTextColor = hover;
        button.selectedTextColor = hover;
        button.inactiveTextColor = Color.white;
        button.selectedInactiveTextColor = Color.white;
        button.disabledTextColor = Color.white;
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((Action)(() => Constants.OpenURL(Url)));
        button.OnMouseOver = new();
        button.OnMouseOut = new();
        button.OnMouseOver.AddListener((Action)(() => text.color = new Color32(70,225,205,255)));
        button.OnMouseOut.AddListener((Action)(() => text.color = Color.white));

        return root;
    }

    public static void Position(MainMenuManager menu)
    {
        var parent = menu.transform.Find("MainUI/AspectScaler") ?? menu.transform;
        var root = parent.Find(ObjectName);
        if(root) PositionRoot(root,.21f);
    }

    private static void PositionRoot(Transform root,float topFraction)
    {
        var camera=Camera.main;
        if (!camera || !root || !root.gameObject.activeInHierarchy) return;
        var collider = root.GetComponent<BoxCollider2D>();
        // Read transform geometry directly: collider bounds can lag until the next
        // physics step and otherwise cause repeated rescaling/jitter in LateUpdate.
        var offset = collider.offset;
        var min = camera.WorldToScreenPoint(root.TransformPoint(new Vector3(offset.x-collider.size.x/2f,offset.y-collider.size.y/2f,0)));
        var max = camera.WorldToScreenPoint(root.TransformPoint(new Vector3(offset.x+collider.size.x/2f,offset.y+collider.size.y/2f,0)));
        if (max.y-min.y < .01f) return;
        var safe = Screen.safeArea;
        var height = safe.height*.05f;
        var factor = height/(max.y-min.y);
        var scale = root.localScale;
        root.localScale = new Vector3(scale.x*factor,scale.y*factor,scale.z);
        var width = height*collider.size.x/collider.size.y;
        var worldCenter = root.TransformPoint(new Vector3(offset.x,offset.y,0));
        var center = camera.WorldToScreenPoint(worldCenter);
        var point = new Vector3(safe.xMax-safe.height*.03f-width/2f,safe.yMax-safe.height*topFraction,center.z);
        root.position += camera.ScreenToWorldPoint(point)-worldCenter;
    }

    private static GameObject Child(Transform parent,string name)
    {
        var child = new GameObject(name);
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent,false);
        return child;
    }
}

[HarmonyLib.HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
public static class LobbyCommunityLink
{
    [HarmonyLib.HarmonyPostfix]
    public static void Postfix(HudManager __instance)=>MenuCommunityLink.EnsureLobby(__instance);
}
