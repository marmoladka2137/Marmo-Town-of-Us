using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Achievements;

/// <summary>
/// How a count achievement's progress persists. Mirrors AchievementsAPI's
/// <c>AchPersistence</c>; kept as a local mirror so this assembly never needs to
/// reference AchievementsAPI for descriptors.
/// </summary>
public enum AchievementPersistence
{
    ThroughoutSessions,
    ThroughoutRounds,
    ResetOnRoundStart,
    ResetOnMeetingStart
}

/// <summary>
/// Describes a single achievement without referencing any AchievementsAPI type.
/// Concrete <c>BaseAchievement</c> instances are materialized from these specs at runtime,
/// only when the AchievementsAPI plugin is present.
/// </summary>
/// <param name="Key">Stable identity used for lookups and locale keys
/// (e.g. <c>{LocalePrefix}.{Key}.Name</c>). Matches the old tab property names.</param>
/// <param name="Name">English display name. Combined with the registering assembly's name it
/// forms the storage id (<c>{AssemblyName}_{Name}</c>), matching previously shipped ids.</param>
/// <param name="Description">English description.</param>
/// <param name="Icon">The achievement's icon.</param>
/// <param name="SubIcon">Optional sub-icon drawn in the achievements menu.</param>
/// <param name="SubIconTooltip">Optional hover tooltip for the sub-icon (e.g. the role or
/// modifier it represents). Resolved through Mira's locale manager at display time, so a
/// registered locale key such as <c>TownOfUsMira.Role.Mayor</c> shows translated text;
/// anything else is shown verbatim.</param>
/// <param name="Rarity">0 = common, 1 = rare, 2 = epic, 3 = legendary.</param>
/// <param name="Hidden">Whether the achievement is hidden until unlocked.</param>
/// <param name="ProgressGoal">Progress to unlock; 0 means this is a simple unlock achievement.</param>
/// <param name="Persistence">How progress persists for count achievements.</param>
/// <param name="ToastOnGameEnd">Show the unlock toast at game end instead of immediately.</param>
/// <param name="Category">Optional per-achievement category override; null uses the owning tab's
/// category (e.g. "crewmate", "impostor", "neutral").</param>
/// <param name="Id">Optional explicit storage id; null defaults to
/// <c>{RegisteringAssembly}_{Name}</c>. Use to preserve save ids when the display name changed.</param>
/// <param name="HideProgress">Hides the progress bar for hidden count achievements.</param>
/// <param name="Tags">Optional tags for filtering and searching (e.g. role names, modifier names).</param>
// ATONY DOCUMENT THIS PATH
public sealed record AchievementSpec(
    string Key,
    string Name,
    string Description,
    LoadableAsset<Sprite> Icon,
    LoadableAsset<Sprite>? SubIcon = null,
    int Rarity = 0,
    bool Hidden = false,
    int ProgressGoal = 0,
    AchievementPersistence Persistence = AchievementPersistence.ThroughoutSessions,
    bool ToastOnGameEnd = false,
    string? Category = null,
    string? Id = null,
    bool HideProgress = false,
    string[]? Tags = null,
    string? SubIconTooltip = null);

/// <summary>
/// Describes an achievements tab (plus default art) a mod's achievements are registered under.
/// Adapts to whatever art/offsets a mod wants for its achievements.
/// </summary>
/// <param name="Assembly">The assembly that owns the achievements; prefixes their storage ids.</param>
/// <param name="TabName">Plain-text fallback name of the tab.</param>
/// <param name="ModName">Name of the mod, used for group headers in the achievements menu.</param>
/// <param name="CategoryId">Shared category id to file this tab's achievements under
/// (e.g. "general", "crewmate", "impostor", "neutral").</param>
/// <param name="TabLocaleKey">Localization key to use for the tabs.</param>
/// <param name="LocalePrefix">Prefix for per-achievement name/description locale keys
/// (<c>{LocalePrefix}.{Key}.Name</c>), or null to use the plain English text.</param>
/// <param name="TabIcon">Icon of the tab in the achievements menu.</param>
/// <param name="ModIcon">Icon shown next to the mod name in group headers.</param>
/// <param name="TabColor">Background color of the achievements menu for this tab.</param>
/// <param name="Art">Optional shared artwork/offsets applied to every achievement of this mod.</param>
// ATONY DOCUMENT THIS PATH
public sealed record AchievementTabSpec(
    Assembly Assembly,
    string TabName,
    string ModName,
    string CategoryId,
    string? TabLocaleKey = null,
    string? LocalePrefix = null,
    LoadableAsset<Sprite>? TabIcon = null,
    LoadableAsset<Sprite>? ModIcon = null,
    Color? TabColor = null,
    AchievementArt? Art = null);

/// <summary>
/// Default artwork and layout offsets shared by all of a mod's achievements.
/// </summary>
// ATONY DOCUMENT THIS PATH
public sealed record AchievementArt(
    LoadableAsset<Sprite>? MenuBg = null,
    LoadableAsset<Sprite>? ToastBg = null,
    Vector3? MenuIconOffset = null,
    Vector3? MenuTitleOffset = null,
    Vector3? MenuDescOffset = null,
    Vector3? MenuSubIconOffset = null,
    Vector3? MenuSubIconScale = null,
    Vector3? ToastIconOffset = null);

/// <summary>
/// Optional integration with the AchievementsAPI plugin. This type contains no references to
/// AchievementsAPI, so it is safe to use anywhere; when the plugin is absent every method is a
/// no-op. Mods that want to hook into Town of Us's achievements integration can register their
/// own achievements with <see cref="RegisterMod"/> and trigger them with the methods below,
/// without taking a dependency on AchievementsAPI themselves.
/// </summary>
/// <remarks>
/// The typed calls live in <c>AchievementsAPIBridge</c>, which is only ever invoked — and thus
/// only JIT-compiled — once <see cref="IsLoaded"/> is known to be true.
/// </remarks>
// ATONY DOCUMENT THIS PATH
// MTOU: achievements removed at the owner's request. Keep this dependency-free
// compatibility facade so gameplay call sites do not acquire achievement side effects.
public static class AchievementsAPIHelper
{
    public const string PluginGuid = "AchievementsAPI";
    public static bool CanAchievementsBeGranted => false;
    public static bool IsLoaded => false;
    public static void RegisterTouAchievements() { }
    public static void RegisterMod(AchievementTabSpec tab, IReadOnlyCollection<AchievementSpec> achievements) { }
    public static void Unlock(AchievementSpec achievement, bool needsToMeetRequirements = true) { }
    public static bool IsUnlocked(AchievementSpec achievement) => false;
    public static void UnlockTentative(AchievementSpec achievement) { }
    public static void ClearTentative() { }
    public static void ResetTentative() { }
    public static int GetCurrentValue(AchievementSpec achievement) => 0;
    public static void Increment(AchievementSpec achievement, int amount = 1, bool showOnUI = true) { }
    public static void SetValue(AchievementSpec achievement, int value, bool showOnUI = true) { }
    public static void Reset(AchievementSpec achievement) { }
    public static bool HasOpenMenu() => false;
}
public static class AchievementSpecExtensions
{
    /// <summary>
    /// Adds tags to an achievement spec. Useful for mod extensions to add role names,
    /// modifier names, or other custom tags for filtering and searching.
    /// </summary>
    /// <param name="spec">The achievement spec to modify.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>A new AchievementSpec with the added tags.</returns>
    public static AchievementSpec WithTags(this AchievementSpec spec, params string[] tags)
    {
        var combinedTags = spec.Tags?.ToList() ?? new List<string>();
        combinedTags.AddRange(tags);
        return spec with { Tags = combinedTags.ToArray() };
    }

    /// <summary>
    /// Adds a single tag to an achievement spec.
    /// </summary>
    /// <param name="spec">The achievement spec to modify.</param>
    /// <param name="tag">The tag to add.</param>
    /// <returns>A new AchievementSpec with the added tag.</returns>
    public static AchievementSpec WithTag(this AchievementSpec spec, string tag)
    {
        return spec.WithTags(tag);
    }
}