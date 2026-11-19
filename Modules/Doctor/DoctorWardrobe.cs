using HarmonyLib;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TouMarmo.Modules.Doctor;

/// <summary>Match-local surgical mask, preserving the player's chosen cosmetics.</summary>
public static class DoctorWardrobe
{
    public const string Visor = "visor_pk01_PaperMaskVisor";
    private static readonly Dictionary<byte, string> Originals = new();

    public static bool HasUniform(PlayerControl p) => p && Originals.ContainsKey(p.PlayerId);
    public static void Apply(PlayerControl p)
    {
        if (!p || p.Data == null) return;
        Originals.TryAdd(p.PlayerId, p.Data.DefaultOutfit.VisorId);
        Dress(p.Data.DefaultOutfit);
        if (p.GetAppearanceType() == TownOfUsAppearances.Default)
            p.RawSetVisor(Visor, p.Data.DefaultOutfit.ColorId);
    }
    public static void Dress(NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        outfit.VisorId = Visor;
    }
    public static void Clear()
    {
        var originals = Originals.ToArray();
        Originals.Clear(); // Appearance getters must stop overriding before restoration.
        foreach (var pair in originals)
        {
            var p = MarmoSession.Player(pair.Key);
            if (!p || p!.Data == null) continue;
            var outfit = p.Data.DefaultOutfit;
            outfit.VisorId = pair.Value;
            if (p.GetAppearanceType() == TownOfUsAppearances.Default)
                p.RawSetVisor(outfit.VisorId, outfit.ColorId);
        }
    }
}

[HarmonyPatch(typeof(AppearanceExtensions), nameof(AppearanceExtensions.GetDefaultAppearance))]
public static class DoctorDefaultUniform
{
    [HarmonyPostfix] public static void Postfix(PlayerControl playerControl, VisualAppearance __result)
    {
        if (DoctorWardrobe.HasUniform(playerControl)) DoctorWardrobe.Dress(__result);
    }
}
[HarmonyPatch(typeof(AppearanceExtensions), nameof(AppearanceExtensions.GetDefaultModifiedAppearance))]
public static class DoctorModifiedUniform
{
    [HarmonyPostfix] public static void Postfix(PlayerControl playerControl, VisualAppearance __result)
    {
        if (DoctorWardrobe.HasUniform(playerControl)) DoctorWardrobe.Dress(__result);
    }
}
