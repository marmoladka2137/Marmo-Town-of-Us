using System.Collections;
using BepInEx.Logging;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TouMarmo.Modifiers;
using TouMarmo.Options;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TouMarmo.Modules.Jester;

public static class JesterRuntime
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.Jester");

    private static bool firstBodyAssigned;

    [RegisterEvent(-1000)]
    public static void Reset(RoundStartEvent e)
    {
        firstBodyAssigned = false;
    }

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent e)
    {
        if (firstBodyAssigned) return;
        if (e.DeathReason == DeathReason.Exile) return;
        if (!OptionGroupSingleton<JesterMarmoOptions>.Instance.FirstBodyArrow.Value) return;

        var victim = e.Player;
        if (!victim) return;

        // TimeLord-style restore cycles raise death events for players who are
        // actually alive again; ignore those so the flag is not wasted at round start.
        if (!victim.HasDied())
        {
            Log.LogInfo($"Ignored death event for {victim.PlayerId} ({e.DeathReason}): player is not actually dead.");
            return;
        }

        if (!MarmoSession.InRound)
        {
            Log.LogInfo($"Ignored death event for {victim.PlayerId}: round is not active.");
            return;
        }

        firstBodyAssigned = true;

        var body = e.DeadBody;
        if (body)
        {
            Assign(body);
            return;
        }

        Coroutines.Start(CoAssign(victim.PlayerId));
    }

    private static void Assign(DeadBody? deadBody)
    {
        if (!deadBody) return;
        var body = deadBody!;
        var assigned = 0;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || !player.AmOwner) continue;
            if (player.Data?.Role is not JesterRole)
            {
                Log.LogInfo($"Local player skipped: role is {player.Data?.Role?.GetType().Name ?? "unknown"}, not Jester.");
                continue;
            }
            if (!MarmoSession.Alive(player))
            {
                Log.LogInfo("Local Jester skipped: not alive.");
                continue;
            }
            if (player.HasModifier<JesterFirstBodyArrowModifier>()) continue;
            player.AddModifier<JesterFirstBodyArrowModifier>(body);
            assigned++;
        }

        Log.LogInfo($"First body in round is {body.ParentId}; arrow assigned to {assigned} local Jester(s).");
    }

    private static IEnumerator CoAssign(byte victimId)
    {
        DeadBody? body = null;
        for (var i = 0; i < 30; i++)
        {
            body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b && b.ParentId == victimId && !b.Reported);
            if (body) break;
            yield return null;
        }

        if (body) Assign(body);
        else Log.LogWarning($"Could not find dead body for player {victimId} to point the Jester arrow at.");
    }
}
