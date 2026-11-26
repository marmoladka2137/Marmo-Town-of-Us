using HarmonyLib;
using MiraAPI;
using UnityEngine;
namespace TouMarmo.Modules;

public static class BuildCompatibility
{
    public const byte ManifestKey=250;
    // Managed assembly MVIDs change with compiled code/resources. DisplayVersion
    // remains user-friendly, but is not a compatible role/options wire schema.
    public static readonly string Manifest="MTOU-BUILD/1|"+
        typeof(TouMarmoPlugin).Assembly.ManifestModule.ModuleVersionId.ToString("N")+"|"+
        typeof(MiraApiPlugin).Assembly.ManifestModule.ModuleVersionId.ToString("N");
    private static readonly Dictionary<uint,string?> Seen=new();
    public static bool Verified(PlayerControl player) => player && Seen.TryGetValue(player.NetId,out var value) && Matches(Manifest,value);
    public static bool Matches(string? expected,string? actual) =>
        !string.IsNullOrWhiteSpace(expected) && string.Equals(expected,actual,StringComparison.Ordinal);
    public static void Record(PlayerControl player,Dictionary<byte,string> info)
    {
        if(player) Seen[player.NetId]=info.GetValueOrDefault(ManifestKey);
    }
    public static string[] IncompatiblePlayers() => PlayerControl.AllPlayerControls.ToArray()
        .Where(p=>p && p.Data && !p.Data.Disconnected && !p.AmOwner &&
            (!Seen.TryGetValue(p.NetId,out var version)||!Matches(Manifest,version)))
        .Select(p=>p.Data.PlayerName).ToArray();
    public static void Clear()=>Seen.Clear();
}

[HarmonyPatch(typeof(LobbyBehaviour),nameof(LobbyBehaviour.Start))]
public static class BuildCompatibilityLobbyPatch
{
    [HarmonyPrefix] public static void Prefix()=>BuildCompatibility.Clear();
}

[HarmonyPatch(typeof(GameStartManager),nameof(GameStartManager.BeginGame))]
public static class BuildCompatibilityStartPatch
{
    [HarmonyPrefix,HarmonyPriority(Priority.First)]
    public static bool Prefix()
    {
        if(!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || TutorialManager.InstanceExists) return true;
        var missing=BuildCompatibility.IncompatiblePlayers();
        if(missing.Length==0) return true;
        MarmoSession.Notify("Cannot start: everyone needs the same MTOU and MiraAPI build. Update or wait for verification: "+string.Join(", ",missing));
        Warning("Start blocked: incompatible or unverified MTOU/MiraAPI build(s).");
        return false;
    }
}
