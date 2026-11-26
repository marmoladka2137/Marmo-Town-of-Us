using AmongUs.Data;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace TouMarmo.Modules.Cosmetics;

// MTOU's own category inside the existing hats wardrobe, including the main menu.
// Rendering uses native ColorChips/SelectHat/ClickEquip, not gameplay minigames.
[HarmonyPatch]
public static class MtouHeadWardrobe
{
    private static HatsTab? owner;
    private static bool showing;
    [HarmonyPatch(typeof(HatsTab),nameof(HatsTab.OnEnable)),HarmonyPrefix,HarmonyPriority(Priority.First)]
    public static void ResetPage(HatsTab __instance) { owner=__instance; showing=false; }
    [HarmonyPatch(typeof(HatsTab),nameof(HatsTab.OnEnable)),HarmonyPostfix]
    public static void ShowEntry(HatsTab __instance)
    {
        if (__instance.transform.Find("MTOU Heads Category")) return;
        var title = __instance.transform.Find("Text")?.GetComponent<TextMeshPro>();
        if (!title) return;
        var root = new GameObject("MTOU Heads Category"); root.layer = __instance.gameObject.layer;
        root.transform.SetParent(__instance.transform,false);
        root.transform.localPosition = new Vector3(.86f,.43f,-60);
        var bg = new GameObject("Background"); bg.layer = root.layer; bg.transform.SetParent(root.transform,false);
        bg.transform.localScale = new Vector3(150,45,1);
        var renderer = bg.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),100);
        renderer.color = new Color(.16f,.22f,.3f,.95f);
        var label = Object.Instantiate(title!,root.transform);
        label.name = "MTOU label"; label.transform.localPosition = new Vector3(0,0,-1); label.transform.localScale = Vector3.one;
        var translator = label.GetComponent<TextTranslatorTMP>(); if(translator) { translator.enabled=false; Object.Destroy(translator); }
        label.text = "MTOU • HEADS"; label.fontSize = label.fontSizeMax = 2.2f; label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.Center; label.rectTransform.sizeDelta = new Vector2(1.5f,.45f);
        var box = root.AddComponent<BoxCollider2D>(); box.size = new Vector2(1.5f,.45f);
        var button = root.AddComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)(() => Toggle(__instance)));
        button.OnMouseOver = new UnityEvent(); button.OnMouseOut = new UnityEvent();
        button.OnMouseOver.AddListener((UnityAction)(() => renderer.color = new Color(.25f,.38f,.5f)));
        button.OnMouseOut.AddListener((UnityAction)(() => renderer.color = new Color(.16f,.22f,.3f,.95f)));
    }
    private static void Toggle(HatsTab tab)
    {
        if (owner==tab && showing) { tab.enabled=false; tab.enabled=true; return; }
        owner=tab; showing=true;
        tab.StopAllCoroutines(); // Stop only this wardrobe's unfinished list population.
        foreach(var chip in tab.ColorChips) if(chip) Object.Destroy(chip.gameObject);
        tab.ColorChips.Clear();
        tab.scroller.ScrollToTop();
        tab.scroller.CalculateAndSetYBounds(HeadHatCollection.Heads.Length,tab.NumPerRow,tab.NumRowsVisible,tab.YOffset);
        tab.currentHat=HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat);
        tab.currentHatIsEquipped=true;
        for(var i=0;i<HeadHatCollection.Heads.Length;i++)
        {
            var entry=HeadHatCollection.Heads[i];
            var hat=HatManager.Instance.GetHatById(entry.ProductId);
            if(!hat || hat.ProductId!=entry.ProductId) continue;
            var chip=Object.Instantiate(tab.ColorTabPrefab,tab.scroller.Inner);
            chip.transform.localPosition=new Vector3(tab.XRange.Lerp(i%tab.NumPerRow/(tab.NumPerRow-1f)),tab.YStart-i/tab.NumPerRow*tab.YOffset,-1);
            chip.Button.OnClick.RemoveAllListeners();
            chip.Button.OnClick.AddListener((UnityAction)(()=>{tab.SelectHat(hat);tab.ClickEquip();}));
            chip.Button.OnMouseOver.RemoveAllListeners();
            chip.Button.OnMouseOver.AddListener((UnityAction)(()=>tab.SelectHat(hat)));
            chip.Button.OnMouseOut.RemoveAllListeners();
            chip.Button.OnMouseOut.AddListener((UnityAction)(()=>tab.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat))));
            chip.Button.ClickMask=tab.scroller.Hitbox;
            chip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
            tab.UpdateMaterials(chip.Inner.FrontLayer,hat);
            hat.SetPreview(chip.Inner.FrontLayer,tab.GetDisplayColor());
            chip.Tag=hat;
            chip.SelectionHighlight.gameObject.SetActive(entry.ProductId==DataManager.Player.Customization.Hat);
            var title = tab.transform.Find("Text")?.GetComponent<TextMeshPro>();
            if (title)
            {
                var name = Object.Instantiate(title!,chip.transform);
                var translate = name.GetComponent<TextTranslatorTMP>();
                if (translate) { translate.enabled=false; Object.Destroy(translate); }
                name.name="MTOU Head Name";
                name.text=entry.Name;
                name.transform.localPosition=new Vector3(0,-.32f,-5);
                name.transform.localScale=Vector3.one;
                name.rectTransform.sizeDelta=new Vector2(.95f,.24f);
                name.alignment=TextAlignmentOptions.Center;
                name.enableAutoSizing=true; name.fontSizeMin=.8f; name.fontSizeMax=1.35f;
                name.enableWordWrapping=false; name.overflowMode=TextOverflowModes.Ellipsis;
            }
            tab.ColorChips.Add(chip);
        }
    }
    [HarmonyPatch(typeof(InventoryTab),nameof(InventoryTab.OnDisable)),HarmonyPostfix]
    public static void Clear(InventoryTab __instance) {if(owner==__instance){owner=null;showing=false;}}
}
