using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs;
using TownOfUs.Patches.Options;
using TouMarmo.Roles;
using UnityEngine;

namespace TouMarmo;

[BepInPlugin(PluginId, PluginName, PluginVersion)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public sealed class TouMarmoPlugin : BasePlugin, IMiraPlugin
{
    public const string PluginId = "pl.marmo.toumarmo";
    public const string PluginName = "Marmo Town of Us";
    public const string PluginVersion = "1.1.0-beta";
    public const string DisplayVersion = "1.1.0 Beta";
    // MTOU release preparation, 2026-11-17. Self-assigned all-client UUID;
    // distinct from the unchanged BepInEx plugin ID. Keep stable across releases.
    public const string LobbyRegistrationGuid = "f3dc9545-e8f3-4a99-a95d-17ae396dd698";

    public Harmony Harmony { get; } = new(PluginId);
    public string OptionsTitleText => "Marmo Town of Us";

    // Names for the custom game-options tabs (MenuCategory.CustomOne/CustomTwo).
    // Without these overrides MiraAPI falls back to "Custom Category 1/2".
    public string CustomOptionMenuNameOne =>
        MiraLocaleManager.Get("TouTabOptionBetterMaps", "Marmo: Map Settings");
    public string CustomOptionMenuOneDescription =>
        MiraLocaleManager.Get("TouTabOptionBetterMapsDesc", "Extra map and task settings");
    public string ModifierMenuDescription =>
        MiraLocaleManager.Get("TouTabOptionModifiersDesc", "Modifier settings");

    public ConfigFile GetConfigFile() => Config;

    public override void Load()
    {
        // Official Among Us 18.0+ helper handles hosting and matchmaking filters.
        // Do not add the +25 host-only protocol flag or manually rewrite packets.
        CurrentModRegistration.ModRegistrationGuidString = LobbyRegistrationGuid;
        Log.LogInfo($"All-client lobby registration: {LobbyRegistrationGuid}");
        // Harmony patches MUST be applied before Town of Us initializes its data objects.
        // Upstream applies Harmony.PatchAll() before recreating PlayerData/SettingsData; the
        // save-file patches change the data file names, and running the reload first made the
        // game read the vanilla profile and save it into the modded profile - which reset the
        // player's nickname and cosmetics on every launch.
        ApplyHarmonyPatches();
        TouMarmo.Modules.Profile.CosmeticSelectionRuntime.Initialize();

        // Initialize integrated Town of Us Mira systems (assets, locale, settings, win conditions, etc.)
        try
        {
            TownOfUs.TownOfUsPlugin.InitializeIntegrated(this);
        }
        catch (Exception e)
        {
            Log.LogError($"Integrated TownOfUs initialization failed: {e}");
        }

        MigrateUniversalModifierOptions();
        MiraLocaleManager.Register(PluginId, "TouMarmo");

        try
        {
            TeamChatPatches.ExtensionTeamChatRegistry.RegisterHandler(new TeamChatPatches.ExtensionTeamChatHandler
            {
                Priority = 60,
                IsChatAvailable = SoulguardRole.LocalCanChat,
                SendMessage = SoulguardRole.RpcBondChat,
                GetDisplayText = () => MiraLocaleManager.Get("TouMarmo.Chat.Bond", "Dirtek Chat"),
                DisplayTextColor = new Color32(101, 230, 179, 255)
            });
        }
        catch (Exception e)
        {
            Log.LogWarning($"Marmo team chat registration failed: {e}");
        }

        ReactorCredits.Register(PluginName, DisplayVersion, false, ReactorCredits.AlwaysShow);

        Log.LogInfo("Marmo Town of Us loaded.");
    }

    private void ApplyHarmonyPatches()
    {
        // Patch all Harmony patches in this unified assembly (TOU + MTOU), excluding known crashy MTOU patches.
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            try
            {
                // These native IL2CPP accessors cause startup crashes; role-team handling remains scoped below.
                if (type.Name is "BanditKillAccessPatch" or "BanditDidWinPatch" or "BanditWinGate") continue;
                if ((type == typeof(TouMarmo.Modules.Profile.QaSaveRoot) || type == typeof(TouMarmo.Modules.Profile.QaSaveWriteBoundary)) && !TouMarmo.Modules.Profile.ProfilePaths.IsQa) continue;
                if (!CustomAttributeData.GetCustomAttributes(type).Any(attribute => attribute.AttributeType == typeof(HarmonyPatch))) continue;
                Harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception error)
            {
                Log.LogError($"Harmony patch {type.FullName} failed: {error}");
            }
        }

        PatchModifierGroupLayout();
    }

    /// <summary>
    /// MiraAPI builds the modifiers list per frame and lays out every option group. Its layout
    /// method is internal, so it is patched by reflection: when the modifier search is active,
    /// groups without a single hit are skipped completely (no header, no empty space).
    /// </summary>
    private void PatchModifierGroupLayout()
    {
        try
        {
            var optionsPatchType = typeof(MiraApiPlugin).Assembly.GetType("MiraAPI.Patches.Options.GameOptionsMenuPatch");
            var updateGroup = optionsPatchType == null ? null : AccessTools.Method(optionsPatchType, "UpdateGroup");
            var prefix = AccessTools.Method(
                typeof(Patches.RoleSettingFilterPatches),
                nameof(Patches.RoleSettingFilterPatches.ModifierGroupPrefix));

            if (updateGroup == null || prefix == null)
            {
                Log.LogWarning("Modifier search: MiraAPI option group layout was not found; empty group headers may stay visible.");
                return;
            }

            new Harmony($"{PluginId}.modifiergroups").Patch(updateGroup, prefix: new HarmonyMethod(prefix));
            Log.LogInfo("Modifier search: modifier groups without a hit will collapse.");
        }
        catch (Exception error)
        {
            Log.LogWarning($"Modifier search group patch failed: {error}");
        }
    }

    private void MigrateUniversalModifierOptions()
    {
        // Existing entries win; defaults are copied only on the first upgrade.
        const string destination = "TouMarmo.Options.UniversalModifiersOptions";
        foreach (var item in new[]
        {
            ("ComposterAddon.Options.BalloonOptions", "Amount", "BalloonAmount", 1f),
            ("ComposterAddon.Options.BalloonOptions", "Chance", "BalloonChance", 0f),
            ("ComposterAddon.Options.BalloonOptions", "Duration", "BalloonDuration", 3f),
            ("TouMarmo.Options.KemadaOptions", "Amount", "KemadaAmount", 1f),
            ("TouMarmo.Options.KemadaOptions", "Chance", "KemadaChance", 0f)
        })
        {
            var previous = Config.Bind(item.Item1, item.Item2, item.Item4).Value;
            Config.Bind(destination, item.Item3, previous);
        }
    }
}







