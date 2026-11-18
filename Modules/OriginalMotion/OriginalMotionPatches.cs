using System.Reflection;
using HarmonyLib;
using PowerTools;
using UnityEngine;

namespace TouMarmo.Modules.OriginalMotion;

internal static class OriginalMotionAdapter
{
    public static bool TryGet(SpriteAnim owner, out OriginalMotionPlayer player)
    { player = owner ? owner.GetComponent<OriginalMotionPlayer>() : null!; return player && player.Spec != null; }
    public static IEnumerable<MethodBase> Methods(params string[] names) => names.Select(n => AccessTools.Method(typeof(SpriteAnim), n)).Where(m => m != null)!;
}

[HarmonyPatch(typeof(SpriteAnim), nameof(SpriteAnim.Play))]
internal static class OriginalMotionPlayPatch
{
    private static bool Prefix(SpriteAnim __instance, AnimationClip __0, float __1)
    {
        if (!OriginalMotionCatalog.Find(__0, out var spec))
        {
            if (OriginalMotionAdapter.TryGet(__instance, out var old)) old.Release();
            return true;
        }
        var player = __instance.GetComponent<OriginalMotionPlayer>() ?? __instance.gameObject.AddComponent<OriginalMotionPlayer>();
        player.Play(__instance, __0, spec, __1);
        return false;
    }
}

[HarmonyPatch]
internal static class OriginalMotionCommandPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => OriginalMotionAdapter.Methods("Stop", "Pause", "Resume", "SetSpeed", "SetTime", "SetNormalizedTime", "set_Paused", "set_Speed", "set_Time", "set_NormalizedTime", "set_FrameTime");
    private static bool Prefix(SpriteAnim __instance, MethodBase __originalMethod, object[] __args)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        switch (__originalMethod.Name)
        {
            case "Stop": p.Clock.Stopped = true; break;
            case "Pause": p.Clock.Paused = true; break;
            case "Resume": p.Clock.Paused = false; break;
            case "set_Paused": p.Clock.Paused = Convert.ToBoolean(__args[0]); break;
            case "SetSpeed": case "set_Speed":
                float speed = Convert.ToSingle(__args[0]);
                p.Clock.Speed = float.IsFinite(speed) ? Math.Max(0, speed) : 0;
                __instance.m_speed = p.Clock.Speed; break;
            case "SetTime": case "set_Time": p.Clock.SetTime(Convert.ToSingle(__args[0])); break;
            case "SetNormalizedTime": case "set_NormalizedTime": p.Clock.SetTime(Convert.ToSingle(__args[0]) * p.Clock.Duration); break;
            case "set_FrameTime": p.Clock.SetTime(Convert.ToInt32(__args[0]) / 24.0); break;
        }
        p.Draw(); return false;
    }
}

[HarmonyPatch(typeof(SpriteAnim), nameof(SpriteAnim.IsPlaying), typeof(AnimationClip))]
internal static class OriginalMotionPlayingClipPatch
{
    private static bool Prefix(SpriteAnim __instance, AnimationClip __0, ref bool __result)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        __result = (!__0 || __instance.m_currAnim == __0) && p.Clock.Playing; return false;
    }
}
[HarmonyPatch(typeof(SpriteAnim), nameof(SpriteAnim.IsPlaying), typeof(string))]
internal static class OriginalMotionPlayingNamePatch
{
    private static bool Prefix(SpriteAnim __instance, string __0, ref bool __result)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        __result = p.Spec.Name == __0 && p.Clock.Playing; return false;
    }
}
[HarmonyPatch]
internal static class OriginalMotionBoolPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => OriginalMotionAdapter.Methods("get_Playing", "get_Paused", "IsPaused");
    private static bool Prefix(SpriteAnim __instance, MethodBase __originalMethod, ref bool __result)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        __result = __originalMethod.Name == "get_Playing" ? p.Clock.Playing : p.Clock.Paused || p.Clock.Speed <= 0;
        return false;
    }
}
[HarmonyPatch]
internal static class OriginalMotionFloatPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => OriginalMotionAdapter.Methods("GetSpeed", "get_Speed", "GetTime", "get_Time", "GetNormalisedTime", "get_NormalizedTime");
    private static bool Prefix(SpriteAnim __instance, MethodBase __originalMethod, ref float __result)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        __result = __originalMethod.Name switch
        {
            "GetSpeed" or "get_Speed" => p.Clock.Speed,
            "GetNormalisedTime" or "get_NormalizedTime" => (float)p.Clock.Normalized,
            _ => (float)(p.Clock.Loop ? p.Clock.Elapsed % p.Clock.Duration : p.Clock.Elapsed)
        };
        return false;
    }
}
[HarmonyPatch(typeof(SpriteAnim), "get_FrameTime")]
internal static class OriginalMotionFramePatch
{
    private static bool Prefix(SpriteAnim __instance, ref int __result)
    {
        if (!OriginalMotionAdapter.TryGet(__instance, out var p)) return true;
        __result = (int)((p.Clock.Loop ? p.Clock.Elapsed % p.Clock.Duration : p.Clock.Elapsed) * 24);
        return false;
    }
}
