using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace TouMarmo.Modules.Profile;

/// <summary>Opt-in test profile redirection. Normal launches retain the existing TOUMira profile.</summary>
public static class ProfilePaths
{
    private static readonly string? RequestedRoot=Environment.GetEnvironmentVariable("MTOU_QA_PROFILE_ROOT");
    public static bool IsQa => !string.IsNullOrWhiteSpace(RequestedRoot);
    public static string Root => IsQa ? Path.GetFullPath(RequestedRoot!) : Application.persistentDataPath;
    public static string SaveSuffix => IsQa ? "_MTOU_QA" : "_TOUMira";
}

[HarmonyPatch]
public static class QaSaveRoot
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> Targets()
    {
        if(!ProfilePaths.IsQa) yield break;
        yield return AccessTools.Method(typeof(Innersloth.IO.FileIO),nameof(Innersloth.IO.FileIO.GetRootDataPath));
        yield return AccessTools.Method(typeof(Innersloth.IO.DefaultFileIO),nameof(Innersloth.IO.DefaultFileIO.GetRootDataPath));
    }
    [HarmonyPrefix]
    public static bool Prefix(ref string __result)
    {
        if(!ProfilePaths.IsQa) return true;
        __result=ProfilePaths.Root;
        return false;
    }
}

/// <summary>Fail closed if a test save escapes the explicitly selected QA directory.</summary>
[HarmonyPatch]
public static class QaSaveWriteBoundary
{
    private static bool notified;
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> Targets()
    {
        if(!ProfilePaths.IsQa) yield break;
        foreach(var type in new[]{typeof(Innersloth.IO.FileIO),typeof(Innersloth.IO.DefaultFileIO)})
            foreach(var method in type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static))
                if(method.Name is "WriteAllText" or "WriteAllBytes") yield return method;
    }
    [HarmonyPrefix]
    public static bool Prefix(string __0)
    {
        var root=Path.GetFullPath(ProfilePaths.Root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
        if(Path.GetFullPath(__0).StartsWith(root,StringComparison.OrdinalIgnoreCase)) return true;
        if(!notified){notified=true;Warning("QA blocked a write outside the isolated profile.");}
        return false;
    }
}
