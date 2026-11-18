using HarmonyLib;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TouMarmo.Modules.Doctor;

/// <summary>Match-local reveal uniform. Never changes the saved lobby profile or inventory.</summary>
public static class DoctorWardrobe
{
    public const string Skin = "skin_Science";
    public const string Hat = "hat_stethescope";
    public const string Visor = "visor_pk01_PaperMaskVisor";
    private static readonly Dictionary<byte, NetworkedPlayerInfo.PlayerOutfit> Originals = new();

    public static bool HasUniform(PlayerControl p) => p && Originals.ContainsKey(p.PlayerId);
    public static void Apply(PlayerControl p)
    {
        if (!p || p.Data == null) return;
        if (!Originals.ContainsKey(p.PlayerId)) Originals[p.PlayerId] = Copy(p.Data.DefaultOutfit);
        Dress(p.Data.DefaultOutfit);
        if (p.GetAppearanceType() == TownOfUsAppearances.Default)
            p.RawSetAppearance(p.GetDefaultModifiedAppearance());
    }
    public static void Dress(NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        outfit.SkinId = Skin; outfit.HatId = Hat; outfit.VisorId = Visor;
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
            outfit.SkinId = pair.Value.SkinId; outfit.HatId = pair.Value.HatId; outfit.VisorId = pair.Value.VisorId;
            if (p.GetAppearanceType() == TownOfUsAppearances.Default)
                p.RawSetAppearance(p.GetDefaultModifiedAppearance());
        }
    }
    private static NetworkedPlayerInfo.PlayerOutfit Copy(NetworkedPlayerInfo.PlayerOutfit o) => new()
    {
        SkinId=o.SkinId, HatId=o.HatId, VisorId=o.VisorId, ColorId=o.ColorId,
        PlayerName=o.PlayerName, PetId=o.PetId, NamePlateId=o.NamePlateId
    };
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
