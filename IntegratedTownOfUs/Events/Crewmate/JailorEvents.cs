// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TouMarmo.Modifiers;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class JailorEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent _)
    {
        var sparedPlayers = ModifierUtils.GetPlayersWithModifier<JailSparedModifier>().ToList();
        sparedPlayers.Do(x => x.RemoveModifier<JailSparedModifier>());

        var players = ModifierUtils.GetPlayersWithModifier<JailedModifier>().ToList();
        if (!OptionGroupSingleton<JailorOptions>.Instance.JailInARow)
        {
            players.Do(x => x.AddModifier<JailSparedModifier>(x.GetModifier<JailedModifier>()!.JailorId));
        }

        players.Do(x => x.RemoveModifier<JailedModifier>());
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if (victim.Data != null && (victim.Data.Role is JailorRole || victim.GetRoleWhenAlive() is JailorRole) && !MeetingHud.Instance)
        {
            ModifierUtils.GetPlayersWithModifier<JailedModifier>().Do(x => x.RemoveModifier<JailedModifier>());
        }

        if (source.Data.Role is not JailorRole jailor)
        {
            return;
        }

        jailor.Executes--;

        if (source.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return;
        }

        var target = @event.Target;

        var incorrect = target.IsCrewmate() && source != target &&
            !(target.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished);
        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (incorrect) stats.IncorrectKills++;
            else stats.CorrectKills++;
        }
        if (!incorrect) return;
        // Every peer decides from the same pre-consumption state, or from the confirmed receipt.
        bool forgiven = OneMoreChanceModifier.WasForgiven(source, target) ||
            source.GetModifier<OneMoreChanceModifier>() is { Used: false };
        if (forgiven)
        {
            if (MarmoSession.IsHost) OneMoreChanceModifier.Consume(source, target);
            return;
        }
        jailor.Executes = 0;
    }
}
