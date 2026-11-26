using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Modifiers;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.GameOver;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles;
using UnityEngine;
namespace TouMarmo.Modules.Doctor;
public sealed class DoctorState
{
    public readonly HashSet<byte> Marks = new();
    public int MarksThisRound;
    public float ReadyAt;
    public bool Revealed;
    public bool Locked;
    public readonly HashSet<byte> Shields = new();
    public PlayerVoteArea? RevealArea;
}
public static class DoctorRuntime
{
    public static readonly Dictionary<byte, DoctorState> States = new();
    public static DoctorOptions Options => OptionGroupSingleton<DoctorOptions>.Instance;
    public static DoctorState For(PlayerControl p)
    {
        if (!States.TryGetValue(p.PlayerId, out var state)) States[p.PlayerId] = state = new();
        return state;
    }
    public static bool CanMark(PlayerControl p, PlayerControl target)
    {
        if (!MarmoSession.Alive(p) || !MarmoSession.Alive(target) || p == target || p.Data.Role is not DoctorRole || !MarmoSession.InRound) return false;
        var s = For(p);
        return !s.Locked && !s.Revealed && !s.Marks.Contains(target.PlayerId) && s.MarksThisRound < TouMarmo.Modules.AbilityLimits.Count(Options.MarksPerRound.Value) && Time.time >= s.ReadyAt &&
            Vector2.Distance(p.GetTruePosition(), target.GetTruePosition()) <= 2.25f;
    }
    /// <summary>
    /// Checks whether reveal conditions are met, WITHOUT checking Locked state.
    /// Used to determine if Locked should be reset.
    /// </summary>
    public static bool CanRevealConditions(PlayerControl p)
    {
        if (!MarmoSession.Alive(p) || p.Data.Role is not DoctorRole || For(p).Revealed) return false;
        var living = PlayerControl.AllPlayerControls.ToArray().Where(x => MarmoSession.Alive(x)).ToArray();

        // When only 2 players alive (Doctor + 1 other), allow reveal regardless of role and marks
        if (living.Length <= 2) return true;

        // Calculate dynamically: only count living crewmates (excluding Doctor) as targets
        var crew = living.Where(x => x != p && x.IsCrewmate()).ToArray();
        if (crew.Length == 0) return true; // No crewmates to mark, but can still reveal

        // Clean up dead players from Marks to keep counter accurate
        var livingIds = new HashSet<byte>(living.Select(x => x.PlayerId));
        var state = For(p);
        var deadMarks = state.Marks.Where(id => !livingIds.Contains(id)).ToList();
        foreach (var id in deadMarks) state.Marks.Remove(id);

        var marked = crew.Count(x => state.Marks.Contains(x.PlayerId));
        return NewRoleRules.DoctorReady(crew.Length, marked, Options.Percent.Value);
    }

    public static bool CanReveal(PlayerControl p)
    {
        if (For(p).Locked) return false;
        return CanRevealConditions(p);
    }
    public static bool Immortal(PlayerControl p)
    {
        if (p == null || p.Data?.Role is not DoctorRole || !For(p).Revealed || !Options.Immortal.Value) return false;
        // Doctor loses immortality when fewer than 3 players are alive
        var alive = PlayerControl.AllPlayerControls.ToArray().Where(x => MarmoSession.Alive(x)).ToArray();
        return AuditRules.DoctorImmortal(For(p).Revealed, Options.Immortal.Value, alive.Length);
    }
    public static bool ExileProtected(PlayerControl p) => p && p.Data?.Role is DoctorRole && For(p).Revealed && !Options.CanBeExiled.Value;
    [MethodRpc(1060, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMarkRequest(PlayerControl p, PlayerControl target)
    {
        if (!MarmoSession.IsHost || !CanMark(p,target)) return;
        var living = PlayerControl.AllPlayerControls.ToArray().Where(x => x != p && MarmoSession.Alive(x) && x.IsCrewmate()).ToArray();
        var marked = living.Count(x => For(p).Marks.Contains(x.PlayerId));
        bool unlocks = target.IsCrewmate() && !NewRoleRules.DoctorReady(living.Length, marked, Options.Percent.Value)
            && NewRoleRules.DoctorReady(living.Length, marked + 1, Options.Percent.Value);
        RpcMark(p, target, !unlocks);
    }
    [MethodRpc(1061, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMark(PlayerControl p, PlayerControl target, bool shield)
    {
        var s = For(p);
        if (s.Revealed || !s.Marks.Add(target.PlayerId)) return;
        s.MarksThisRound++; s.ReadyAt = Time.time + Options.Cooldown.Value;
        BigBrainRuntime.Record(p, target, false);
        if (shield)
        {
            s.Shields.Add(target.PlayerId);
            if (!target.HasModifier<DoctorShieldModifier>()) target.AddModifier<DoctorShieldModifier>();
        }
    }
    public static bool MayAttempt(PlayerControl p)
    {
        if (!MarmoSession.Alive(p) || p.Data.Role is not DoctorRole || For(p).Revealed || !MeetingHud.Instance ||
            MeetingHud.Instance.CurrentState is not (MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted))
        {
            return false;
        }

        // Reset Locked if conditions can now be met (e.g., a marked player died)
        if (For(p).Locked && CanRevealConditions(p))
        {
            For(p).Locked = false;
        }

        return !For(p).Locked && !p.HasModifier<JailedModifier>();
    }
    [MethodRpc(1062, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRevealRequest(PlayerControl p)
    {
        if (!MarmoSession.IsHost || !MayAttempt(p)) return;
        if (CanReveal(p)) RpcReveal(p);
        else RpcFailedReveal(p);
    }
    [MethodRpc(1065, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFailedReveal(PlayerControl p)
    {
        // Only lock if reveal is truly impossible (not in 1v1 where reveal is always allowed)
        var living = PlayerControl.AllPlayerControls.ToArray().Where(x => MarmoSession.Alive(x)).ToArray();
        if (living.Length > 2)
        {
            For(p).Locked = true;
            if (p.AmOwner) MarmoSession.Notify("Too few marked Crewmates. Marking is disabled for the next round.");
        }
    }
    [MethodRpc(1063, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcReveal(PlayerControl p)
    {
        if (!p || p.Data == null) return;
        var s = For(p);
        if (s.Revealed || p.Data.Role is not DoctorRole) return;
        s.Revealed = true;
        DoctorWardrobe.Apply(p);
        var area = MeetingHud.Instance ? MeetingHud.Instance.playerStates.FirstOrDefault(x => x.PlayerId == p.PlayerId) : null;
        if (area)
        {
            s.RevealArea = area;
            var icon = area!.PlayerIcon;
            if (icon) icon!.UpdateFromPlayerOutfit(p.Data.DefaultOutfit, PlayerMaterial.MaskType.ComplexUI, false, false, null, false);
            DoctorRevealPresentation.Play(area!, true);
        }
    }
    [MethodRpc(1064, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBreakShield(PlayerControl target)
    {
        foreach (var state in States.Values) state.Shields.Remove(target.PlayerId);
        if (target.HasModifier<DoctorShieldModifier>()) target!.RemoveModifier<DoctorShieldModifier>();
    }
    private static void DropShields(DoctorState state)
    {
        foreach (var id in state.Shields.ToArray())
        {
            state.Shields.Remove(id);
            if (States.Values.Any(other => other.Shields.Contains(id))) continue;
            var target = MarmoSession.Player(id);
            if (target && target!.HasModifier<DoctorShieldModifier>()) target!.RemoveModifier<DoctorShieldModifier>();
        }
    }
    [RegisterEvent] public static void Death(PlayerDeathEvent e)
    {
        if (States.TryGetValue(e.Player.PlayerId, out var state))
        {
            DropShields(state);
            if (state.RevealArea)
            {
                var model = state.RevealArea!.transform.Find("MarmoDoctorReveal");
                if (model) UnityEngine.Object.Destroy(model.gameObject);
                var label = state.RevealArea.transform.Find("MarmoDoctorRevealLabel");
                if (label) UnityEngine.Object.Destroy(label.gameObject);
                if (state.RevealArea.PlayerIcon) state.RevealArea.PlayerIcon.gameObject.SetActive(true);
            }
        }
        foreach (var owner in States.Values) owner.Shields.Remove(e.Player.PlayerId);
    }
    [RegisterEvent] public static void Meeting(StartMeetingEvent e)
    {
        foreach (var state in States.Values) state.Locked = false;
    }
    [RegisterEvent(2000)] public static void Shield(BeforeMurderEvent e)
    {
        if (!ProtectionRuntime.MayDefend(e, e.Target)) return;
        if (Immortal(e.Target)) { ProtectionRuntime.Block(e, e.Source, e.Target, ProtectionKind.Doctor, e.Target); return; }
        if (e.IgnoreDefense || e.Source == e.Target || !e.Target.HasModifier<DoctorShieldModifier>()) return;
        if (!ProtectionRuntime.Block(e, e.Source, e.Target, ProtectionKind.Doctor, States.Where(pair => pair.Value.Shields.Contains(e.Target.PlayerId)).Select(pair => MarmoSession.Player(pair.Key)).FirstOrDefault(p => MarmoSession.Alive(p)))) return;
        if (MarmoSession.IsHost) RpcBreakShield(e.Target);
    }
    [RegisterEvent(2000)] public static void Vote(ProcessVotesEvent e) { if (e.ExiledPlayer?.Object is { } p && ExileProtected(p)) e.ExiledPlayer = null; }
    [RegisterEvent] public static void Round(RoundStartEvent e) { foreach(var s in States.Values) s.MarksThisRound = 0; }
    private static void TryEndGameIfDoctorAloneWithEvil()
    {
        // Do not force game end based on player count and faction.
        // Preserve standard TOU Mira win conditions.
        // The game's normal win condition checks will handle game end.
    }

    public static void Tick()
    {
        foreach (var pair in States)
        {
            if (pair.Value.Shields.Count == 0) continue;
            var owner = MarmoSession.Player(pair.Key);
            if (!MarmoSession.Alive(owner) || owner!.Data.Role is not DoctorRole) DropShields(pair.Value);
        }
        TryEndGameIfDoctorAloneWithEvil();

        if (!MeetingHud.Instance) return;
        foreach(var area in MeetingHud.Instance.playerStates)
        {
            var p = MarmoSession.Player(area.PlayerId.Value);
            if (!p || p!.Data.Role is not DoctorRole || !For(p).Revealed || area.AmDead) continue;
            var state = For(p);
            if (state.RevealArea != area)
            {
                state.RevealArea = area;
                DoctorRevealPresentation.Play(area, false);
            }
            else if (area.PlayerIcon && area.transform.Find("MarmoDoctorReveal") is { } model && model.gameObject.activeSelf)
                area.PlayerIcon.gameObject.SetActive(false);
        }
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class DoctorDirectMurderPatch
{
    [HarmonyPrefix] public static bool Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        if (ProtectionRuntime.Bypass(target) || !DoctorRuntime.Immortal(target)) return true;
        if (resultFlags.HasFlag(MurderResultFlags.Succeeded))
            ProtectionRuntime.ShowLocal(__instance, target, ProtectionKind.Doctor, target);
        return false;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class DoctorExilePatch
{
    [HarmonyPrefix] public static bool Prefix(PlayerControl __instance) => ProtectionRuntime.BondDeaths.Contains(__instance.PlayerId) || !DoctorRuntime.ExileProtected(__instance);
}


[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateStatusSymbols),
    typeof(string), typeof(PlayerControl), typeof(DataVisibility))]
public static class DoctorNameSymbols
{
    [HarmonyPostfix] public static void Postfix(PlayerControl player, DataVisibility visibility, ref string __result)
    {
        if (!player || visibility == DataVisibility.Hidden) return;
        var local = PlayerControl.LocalPlayer;
        if (MarmoSession.Alive(local) && local.Data.Role is DoctorRole && DoctorRuntime.For(local).Marks.Contains(player.PlayerId))
            __result += " <color=#46CADC>+</color>";
    }
}
