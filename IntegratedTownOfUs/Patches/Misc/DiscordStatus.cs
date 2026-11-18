// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using BepInEx.Unity.IL2CPP;
using Discord;
using HarmonyLib;
using System;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

/// <remarks>
/// Patch taken from <see href="https://github.com/All-Of-Us-Mods/LaunchpadReloaded/blob/master/LaunchpadReloaded/Patches/Generic/DiscordManagerPatch.cs"/>
/// Modified for Marmo Town of Us custom Discord Rich Presence.
/// </remarks>
[HarmonyPatch(typeof(ActivityManager))]
public static class DiscordStatus
{
    private static readonly DateTime LaunchTime = DateTime.UtcNow;
    private static readonly string ModName = "Marmo Town of Us";
    private static readonly string ModVersion = "MTOU " + TouMarmo.TouMarmoPlugin.DisplayVersion;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ActivityManager.UpdateActivity))]
    public static void ActivityManagerUpdateActivityPrefix(ActivityManager __instance, [HarmonyArgument(0)] Activity activity)
    {
        // Set name and details
        activity.Details = ModVersion;
        
        // Determine state based on game context
        activity.State = GetState();
        
        // Set timestamps (time since launch)
        activity.Timestamps = new ActivityTimestamps { Start = ToUnixTime(LaunchTime) };
        
        // Set large image (custom MTOU logo - must be uploaded to Discord Developer Portal)
        activity.Assets = new ActivityAssets
        {
            LargeImage = "mtou_logo",
            LargeText = ModName,
            SmallText = ModName
        };
    }

    private static string GetState()
    {
        try
        {
            if (LobbyBehaviour.Instance != null)
            {
                // In lobby
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                {
                    var playerCount = PlayerControl.AllPlayerControls.Count;
                    var maxPlayers = GameData.Instance != null ? GameData.Instance.PlayerCount : playerCount;
                    return $"In Lobby • {playerCount}/{maxPlayers} Players";
                }
                else
                {
                    return "In Lobby";
                }
            }
            
            if (GameManager.Instance != null)
            {
                // In game
                return "In Game";
            }
            
            // In menus (default)
            return "In Menus";
        }
        catch
        {
            return "In Menus";
        }
    }

    private static long ToUnixTime(DateTime dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}
