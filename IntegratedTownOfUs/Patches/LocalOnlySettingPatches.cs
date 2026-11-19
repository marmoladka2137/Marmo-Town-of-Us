using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LocalSettings
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void HideGhosts(PlayerControl __instance)
    {
        if (!__instance || !__instance.AmOwner || !AmongUsClient.Instance ||
            !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            return;
        }

        if (MeetingHud.Instance)
        {
            return;
        }

        if (!OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player || !player.Data || !player.cosmetics) continue;
            if (player.AmOwner)
            {
                continue;
            }

            if (!player.Data.IsDead)
            {
                continue;
            }

            switch (player.Data.Role)
            {
                case SpectreRole { Caught: false }:
                case HaunterRole { Caught: false }:
                    continue;
            }

            var show = LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.DeadSeeGhostsToggle.Value;
            if (player.transform.childCount < 2) continue;
            var bodyForms = player.gameObject.transform.GetChild(1).gameObject;

            foreach (var form in bodyForms.GetAllChildren())
            {
                if (form.activeSelf)
                {
                    var renderer = form.GetComponent<SpriteRenderer>();
                    if (renderer) renderer.color = new Color(1f, 1f, 1f, show ? 1f : 0f);
                }
            }

            if (player.cosmetics.HasPetEquipped() && player.cosmetics.CurrentPet)
            {
                player.cosmetics.CurrentPet.Visible = show;
            }

            player.cosmetics.gameObject.SetActive(show);
            if (player.transform.childCount > 3) player.gameObject.transform.GetChild(3).gameObject.SetActive(show);
        }

        if (OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility is SentryDeployedCamerasVisibility.AfterMeeting)
        {
            foreach (var cameraPair in SentryRole.Cameras)
            {
                if (cameraPair.Key == null || cameraPair.Key.gameObject == null)
                {
                    continue;
                }

                cameraPair.Key.gameObject.SetActive(true);
                var spriteRenderer = cameraPair.Key.gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer?.color = Color.white;
            }
        }
    }
}
