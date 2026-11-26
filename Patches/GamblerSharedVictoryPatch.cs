using HarmonyLib;
using MiraAPI.GameEnd;
using TownOfUs.GameOver;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;
using TouMarmo.Roles;

namespace TouMarmo.Patches;

// Add the co-winner after Mira has resolved the actual winners, not while
// evaluating role conditions (which could recursively grant an early victory).
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class GamblerSharedVictoryPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(EndGameResult endGameResult)
    {
        bool cancelled = endGameResult.GameOverReason == CustomGameOver.GameOverReason<DrawGameOver>() ||
            endGameResult.GameOverReason == CustomGameOver.GameOverReason<HostGameOver>();
        if (cancelled || !RoleManager.Instance || !GameData.Instance) return;
        var winners = EndGameResult.CachedWinners;
        bool neutralWinner = winners.ToArray().Any(winner =>
        {
            var role = RoleManager.Instance.GetRole(winner.RoleWhenAlive);
            return role && role is not GamblerRole && role.IsNeutral();
        });
        if (!TouMarmo.Modules.FeedbackRules.GamblerSharedWin(neutralWinner, cancelled)) return;
        bool addedWinner = false;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || !player.Data || player.Data.Disconnected || player.GetRoleWhenAlive() is not GamblerRole gambler) continue;
            gambler.HasWon = true;
            if (winners.ToArray().Any(winner => winner.PlayerName == player.Data.PlayerName &&
                winner.ColorId == player.Data.DefaultOutfit.ColorId &&
                RoleManager.Instance.GetRole(winner.RoleWhenAlive) is GamblerRole)) continue;
            winners.Add(new CachedPlayerData(player.Data));
            addedWinner = true;
        }
        // ToU may have built its statistics before this final winners pass.
        if (addedWinner) TownOfUs.Patches.EndGamePatches.BuildEndGameData();
    }
}
