// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using System.Globalization;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.Data.Settings;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Localization;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Achievements;
using TownOfUs.Patches;
using TownOfUs.Patches.Misc;
using TownOfUs.Patches.WinConditions;
using ModCompatibility = TownOfUs.Modules.ModCompatibility;

namespace TownOfUs;

/// <summary>
///     Town of Us Mira integration state/helpers.
///     In the unified Marmo Town of Us build this is not a standalone BepInEx plugin;
///     initialization is driven by the unified host plugin.
/// </summary>
public sealed class TownOfUsPlugin : BasePlugin
{
    // This type exists for compatibility with upstream generic constraints/loggers.
    // It is NOT registered as a standalone BepInEx plugin in the unified build.
    public override void Load() { }

    // Preserved upstream identity (for locale/compat references).
    public const string Id = "auavengers.tou.mira";
    public const string Name = "Town of Us Mira";
    public const string Version = "1.7.9-integrated";

    public static bool IsMobile => Constants.GetPlatformType() is Platforms.Android or Platforms.IPhone;
    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; internal set; } = new("en-US");

    /// <summary>
    ///     Gets the Harmony instance for TOU-origin patches when needed.
    /// </summary>
    public static Harmony Harmony { get; } = new(Id);

    /// <summary>
    ///     Determines if the current build is a dev build or not.
    /// </summary>
    public static bool IsDevBuild => IsBetaBuild || IsWipBuild;

    /// <summary>
    ///     Determines if the current build is a beta build.
    /// </summary>
    public static bool IsBetaBuild => Version.Contains("beta", StringComparison.OrdinalIgnoreCase) || Version.Contains("prerelease", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Determines if the current build is a WIP/dev build.
    /// </summary>
    public static bool IsWipBuild => Version.Contains("dev", StringComparison.OrdinalIgnoreCase) || Version.Contains("ci", StringComparison.OrdinalIgnoreCase) || Version.Contains("integrated", StringComparison.OrdinalIgnoreCase);

    public static string OptionsTitleText => "TOU Mira";
    public static string CustomOptionMenuNameOne => MiraLocaleManager.Get("TouTabOptionBetterMaps");
    public static string CustomOptionMenuOneDescription => MiraLocaleManager.Get("TouTabOptionBetterMapsDesc");
    public static string ModifierMenuDescription => MiraLocaleManager.Get("TouTabOptionModifiersDesc");

    public static ConfigFile Config { get; private set; } = null!;
    public static ConfigEntry<LegacyVisuals> LegacyMode { get; private set; } = null!;
    public static ConfigEntry<bool> AutoDownloadCosmetics { get; private set; } = null!;

    public static ConfigFile GetConfigFile() => Config;

    /// <summary>
    /// Initializes Town of Us Mira systems inside the unified mod host.
    /// </summary>
    public static void InitializeIntegrated(BasePlugin host)
    {
        Config = host.Config;

        // Localization provider registration (kept from upstream behavior).
        LocalizationManager.Register(new TouLocalizationProvider());

        LegacyMode = Config.Bind("LocalSettings", "LegacyMode", LegacyVisuals.Disabled,
            "If enabled, assets will appear like they did in TOU Reactivated / Polus.gg / Town of Us.");
        AutoDownloadCosmetics = Config.Bind("LocalSettings", "AutoDownloadCosmetics", true,
            "If enabled, cosmetics will automatically be downloaded for Corsac Cosmetics.");

        // ReactorCredits.Register("Town Of Us: Mira", Version, IsDevBuild, ReactorCredits.AlwaysShow); // Hidden: integrated into TOU-Marmo
        LocalizationManager.Register(new TaskProvider());
        MiraLocaleManager.Register(Id, "TownOfUs");

        TouAssets.Initialize();
        // MTOU achievements integration removed.

        IL2CPPChainloader.Instance.Finished +=
            ModCompatibility
                .Initialize; // Initialise AFTER the mods are loaded to ensure maximum parity (no need for the soft dependency either then)

        IL2CPPChainloader.Instance.Finished +=
            ModNewsFetcher
                .CheckForNews; // Checks for mod announcements after everything is loaded to avoid Epic Games crashing

        // No background achievement lottery.

        // Unified host controls Harmony patching for the whole assembly.
        // Harmony.PatchAll() is intentionally not called here.

        // Let the game's native data lifecycle load its existing objects. Replacing
        // these mid-startup can discard the loaded customization/settings state.

        RegisterWinConditions();
        RoleExclusionRegistry.RegisterBuiltInBuckets();
    }

    /// <summary>
    ///     Registers all built-in win conditions.
    /// </summary>
    public static void RegisterWinConditions()
    {
        WinConditionRegistry.Register(new NeutralRoleWinCondition());
        WinConditionRegistry.Register(new LoversWinCondition());
    }
}

public enum LegacyVisuals
{
    Disabled,
    /*Players,*/
    Art,
    /*Full*/
}
