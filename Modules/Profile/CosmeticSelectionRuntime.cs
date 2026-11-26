using AmongUs.Data;
using AmongUs.Data.Player;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace TouMarmo.Modules.Profile;

public static class CosmeticSelectionRuntime
{
    public static int EquipDepth;
    internal static CosmeticSelectionStore? Store;
    private static bool pending;
    private static float nextCheck;
    public static void Initialize()
    {
        try
        {
            Store=new CosmeticSelectionStore(Path.Combine(ProfilePaths.Root,"mtou-cosmetic-selection.json"));
            var profile=Path.Combine(ProfilePaths.Root,"player.amogus"+ProfilePaths.SaveSuffix);
            if(File.Exists(profile)) Store.Seed(File.ReadAllText(profile));
            pending=Store.Values.Count>0;
        }
        catch(Exception error){Warning($"Cosmetic preference load failed: {error.GetType().Name}");}
    }
    public static void Protect(PlayerData player)
    {
        var store=Store;
        var customization=player?.Customization;
        if(EquipDepth>0 || store==null || customization==null) return;
        foreach(var pair in store.Values) Write(customization,pair.Key,pair.Value);
    }
    public static void Capture(string slot)
    {
        if(TouMarmo.Modules.LobbyMimic.MimicRuntime.Active || Store==null || DataManager.player==null) return;
        try{Store.Choose(slot,Read(DataManager.Player.Customization,slot));pending=true;}
        catch(Exception error){Warning($"Cosmetic preference save failed: {error.GetType().Name}");}
    }
    private static string Read(PlayerCustomizationData data,string slot)=>slot switch
    {
        "hat"=>data.hat,"skin"=>data.skin,"visor"=>data.visor,"pet"=>data.pet,"namePlate"=>data.namePlate,_=>string.Empty
    };
    private static void Write(PlayerCustomizationData data,string slot,string value)
    {
        // Native backing fields avoid treating startup restoration as a fresh UI equip.
        switch(slot){case "hat":data.hat=value;break;case "skin":data.skin=value;break;case "visor":data.visor=value;break;case "pet":data.pet=value;break;case "namePlate":data.namePlate=value;break;}
    }
    private static bool Resolved(string slot,string id)
    {
        if(string.IsNullOrEmpty(id)) return true;
        var manager=HatManager.Instance;
        if(!manager) return false;
        return slot switch
        {
            "hat"=>manager.GetHatById(id)?.ProdId==id,
            "skin"=>manager.GetSkinById(id)?.ProdId==id,
            "visor"=>manager.GetVisorById(id)?.ProdId==id,
            "pet"=>manager.GetPetById(id)?.ProdId==id,
            "namePlate"=>manager.GetNamePlateById(id)?.ProdId==id,
            _=>false
        };
    }
    public static void Tick()
    {
        if(TouMarmo.Modules.LobbyMimic.MimicRuntime.Active || !pending || Store==null || Time.unscaledTime<nextCheck || DataManager.player==null) return;
        nextCheck=Time.unscaledTime+1;
        // Custom cosmetics may be installed asynchronously; never replace them with empty ids.
        if(Store.Values.Any(p=>!Resolved(p.Key,p.Value))) return;
        Protect(DataManager.Player);
        if(LobbyBehaviour.Instance && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.AmOwner)
        {
            var player=PlayerControl.LocalPlayer;
            var saved=DataManager.Player.Customization;
            player.RpcSetHat(saved.Hat);player.RpcSetSkin(saved.Skin);player.RpcSetVisor(saved.Visor);player.RpcSetPet(saved.Pet);
        }
        pending=false;
    }
}

[HarmonyPatch(typeof(PlayerData),nameof(PlayerData.OnSaveStart))]
public static class PreserveSelectedCosmeticsOnSave
{
    [HarmonyPrefix] public static void Prefix(PlayerData __instance)=>CosmeticSelectionRuntime.Protect(__instance);
}
[HarmonyPatch(typeof(PlayerData),nameof(PlayerData.OnLoadComplete))]
public static class RestoreSelectedCosmeticsOnLoad
{
    [HarmonyPostfix] public static void Postfix(PlayerData __instance)=>CosmeticSelectionRuntime.Protect(__instance);
}

[HarmonyPatch]
public static class RememberExplicitWardrobeEquip
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> Targets()
    {
        foreach(var type in new[]{typeof(HatsTab),typeof(SkinsTab),typeof(VisorsTab),typeof(PetsTab),typeof(NameplatesTab)})
            yield return AccessTools.Method(type,"ClickEquip");
    }
    [HarmonyPrefix] public static void Prefix(out int __state){__state=CosmeticSelectionRuntime.EquipDepth;CosmeticSelectionRuntime.EquipDepth++;}
    [HarmonyPostfix]
    public static void Postfix(MethodBase __originalMethod)
    {
        var slot=__originalMethod.DeclaringType?.Name switch
        {"HatsTab"=>"hat","SkinsTab"=>"skin","VisorsTab"=>"visor","PetsTab"=>"pet","NameplatesTab"=>"namePlate",_=>null};
        if(slot!=null) CosmeticSelectionRuntime.Capture(slot);
    }
    [HarmonyFinalizer]
    public static Exception? Finalizer(int __state,Exception? __exception){CosmeticSelectionRuntime.EquipDepth=__state;return __exception;}
}
