using System.Reflection;
using HarmonyLib;
using TownOfUs.Utilities.Appearances;
namespace TouMarmo.Modules.LobbyMimic;

[HarmonyPatch]
public static class MimicAppearance
{
    public static IEnumerable<MethodBase> TargetMethods() => new[]{"GetDefaultAppearance","GetDefaultModifiedAppearance","GetAppearance"}
        .Select(n=>AccessTools.Method(typeof(AppearanceExtensions),n));
    [HarmonyPostfix,HarmonyPriority(Priority.Last)]
    public static void Postfix(PlayerControl __0,VisualAppearance __result)
    {
        if(MimicRuntime.Locked(__0) && MimicRuntime.Session.Target is { } target) MimicRuntime.Write(__result,target);
    }
}
[HarmonyPatch(typeof(AppearanceExtensions),nameof(AppearanceExtensions.RawSetAppearance),new[]{typeof(PlayerControl),typeof(VisualAppearance)})]
public static class MimicRawAppearance
{
    [HarmonyPrefix]
    public static void Prefix(PlayerControl player,ref VisualAppearance appearance)
    {
        if(!MimicRuntime.Locked(player) || MimicRuntime.Session.Target is not { } target) return;
        appearance=new VisualAppearance(appearance,appearance.AppearanceType);
        MimicRuntime.Write(appearance,target);
    }
}

// Keep real network identities/default names intact: spectator assignment and
// role mechanics can key off those. Only UI consumes a temporary outfit copy.
public static class MimicUiAppearance
{
    private static int nextScope;
    private static readonly Dictionary<int,(NetworkedPlayerInfo.PlayerOutfit Outfit,OutfitSnapshot Saved)> scopes=new();
    public static NetworkedPlayerInfo.PlayerOutfit Copy(NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        if(!MimicRuntime.Active || MimicRuntime.Session.Target is not { } target) return outfit;
        var copy=new VisualAppearance(outfit,TownOfUsAppearances.Default);
        MimicRuntime.Write(copy,target);return copy;
    }
    public static int Push(NetworkedPlayerInfo data)
    {
        if(!data || !MimicRuntime.Locked(data.Object) || MimicRuntime.Session.Target is not { } target) return 0;
        var id=++nextScope;
        scopes.Add(id,(data.DefaultOutfit,MimicRuntime.Read(data.DefaultOutfit)));
        MimicRuntime.Write(data.DefaultOutfit,target);return id;
    }
    public static void Pop(int id)
    {
        if(scopes.Remove(id,out var entry)) MimicRuntime.Write(entry.Outfit,entry.Saved);
    }
    public static void Name(ref string name)
    {
        var originalName=name;
        if(MimicRuntime.Active && MimicRuntime.Session.Target is { } target &&
            MimicRuntime.Session.Originals.Values.Any(o=>o.Name.Equals(originalName,StringComparison.Ordinal))) name=target.Name;
    }
}
[HarmonyPatch(typeof(PoolablePlayer),nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
public static class MimicAvatar
{
    [HarmonyPrefix] public static void Prefix(ref NetworkedPlayerInfo.PlayerOutfit __0) => __0=MimicUiAppearance.Copy(__0);
}
[HarmonyPatch]
public static class MimicUiCosmetics
{
    public static IEnumerable<MethodBase> TargetMethods() => new[]{typeof(PlayerVoteArea),typeof(ChatBubble)}
        .Select(t=>AccessTools.Method(t,"SetCosmetics"));
    [HarmonyPrefix] public static void Prefix(NetworkedPlayerInfo __0,out int __state) => __state=MimicUiAppearance.Push(__0);
    [HarmonyFinalizer] public static Exception? Finalizer(int __state,Exception? __exception)
    {MimicUiAppearance.Pop(__state);return __exception;}
}
[HarmonyPatch]
public static class MimicUiNames
{
    public static IEnumerable<MethodBase> TargetMethods() => new[]{typeof(PoolablePlayer),typeof(ChatBubble)}
        .SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        .Where(m=>m.Name=="SetName" && m.GetParameters().FirstOrDefault()?.ParameterType==typeof(string));
    [HarmonyPrefix] public static void Prefix(ref string __0) => MimicUiAppearance.Name(ref __0);
}

[HarmonyPatch]
public static class MimicCosmeticLock
{
    public static IEnumerable<MethodBase> TargetMethods() => typeof(PlayerControl)
        .GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)
        .Where(m=>new[]{"SetName","SetColor","SetHat","SetSkin","SetVisor","SetPet","SetNamePlate"}.Contains(m.Name));
    [HarmonyPrefix,HarmonyPriority(Priority.First)]
    public static bool Prefix(PlayerControl __instance) => !MimicRuntime.Locked(__instance);
}
[HarmonyPatch]
public static class MimicWardrobeLock
{
    public static IEnumerable<MethodBase> TargetMethods() => new[]{typeof(HatsTab),typeof(SkinsTab),typeof(VisorsTab),typeof(PetsTab),typeof(NameplatesTab),typeof(PlayerTab)}
        .Select(t=>AccessTools.Method(t,"ClickEquip")).Where(m=>m!=null).Cast<MethodBase>()
        .Append(AccessTools.Method(typeof(PlayerTab),nameof(PlayerTab.SelectColor)));
    [HarmonyPrefix,HarmonyPriority(Priority.First)] public static bool Prefix()=>!MimicRuntime.Active;
}
