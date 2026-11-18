// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Patches.WinConditions;

/// <summary>
///     Win condition for neutral roles.
///     Checks all neutral roles and triggers game over for the first one that meets its win condition.
///     Handles special case where Jester/Executioner/Doomsayer with Lover modifier should trigger Lovers win instead.
/// </summary>
public sealed class NeutralRoleWinCondition : IWinCondition
{
    /// <summary>
    ///     Priority 5 - executes before lovers (10) and crew/impostor.
    /// </summary>
    public int Priority => 5;

    /// <summary>
    ///     Checks if any neutral role win condition is met.
    /// </summary>
    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        return WinningPlayer() != null;
    }

    private static PlayerControl? WinningPlayer() => PlayerControl.AllPlayerControls.ToArray()
        .Where(p => p && p.Data && (TouMarmo.Modules.Jester.JesterExileRuntime.EndsGame(p) ||
            p.Data.Role is ICustomRole { Team: ModdedRoleTeams.Custom } && p.Data.Role is ITownOfUsRole r && r.WinConditionMet()))
        .OrderBy(p => TouMarmo.Modules.Jester.JesterExileRuntime.EndsGame(p) ? RoleAlignment.NeutralEvil : p.Data.Role.GetRoleAlignment())
        .FirstOrDefault();

    /// <summary>
    ///     Triggers the neutral game over, or lovers game over if applicable.
    /// </summary>
    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        var player = WinningPlayer();
        if (player != null) TriggerFor(player);
    }

    public static bool TryEndingJester()
    {
        var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p && p.Data && TouMarmo.Modules.Jester.JesterExileRuntime.EndsGame(p));
        if (player == null) return false;
        TriggerFor(player);
        return true;
    }

    private static void TriggerFor(PlayerControl player)
    {
        var winner = player?.Data.Role;

        if (winner == null)
        {
            return;
        }

        // Lovers + Jest/Exe/Doom combo fix
        // If the winner is Jester, Executioner, or Doomsayer and has Lover modifier,
        // check if we should trigger Lovers win instead
        if ((TouMarmo.Modules.Jester.JesterExileRuntime.EndsGame(player) || winner is JesterRole || winner is ExecutionerRole || winner is DoomsayerRole) &&
            player != null && player.HasModifier<LoverModifier>())
        {
            var loverWinners = ModifierUtils.GetActiveModifiers<LoverModifier>()
                .Where(x => x.Player != null && x.Player.Data != null && x.Player.HasModifier<LoverModifier>())
                .Select(x => x.Player!.Data)
                .Distinct()
                .ToArray();

            if (loverWinners.Length >= 2)
            {
                Error($"Game ended from Lovers win caused by a Neutral Evil!");
                CustomGameOver.Trigger<LoverGameOver>(loverWinners);
                return;
            }
        }

        if (player != null)
        {
            var roleName = TouMarmo.Modules.Jester.JesterExileRuntime.EndsGame(player) ? "Jester" : winner.GetRoleName();
            Error($"Game ended from {roleName} win! ({player.Data.PlayerName})");
            CustomGameOver.Trigger<NeutralGameOver>([player.Data]);
        }
    }
}
