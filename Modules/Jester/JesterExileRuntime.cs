using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;

namespace TouMarmo.Modules.Jester;

public static class JesterExileRuntime
{
    public static readonly JesterExileState State = new();
    public static bool HasWon(PlayerControl? player) => player && State.HasWon(player!.PlayerId);
    public static bool EndsGame(PlayerControl? player) => player && State.EndsGame(player!.PlayerId);
    [RegisterEvent(-10000)] public static void Start(RoundStartEvent e) { if(e.TriggeredByIntro) State.Clear(); }
}

[HarmonyPatch(typeof(MeetingHud))]
public static class JesterVoteSnapshotPatch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First), HarmonyPatch(nameof(MeetingHud.Start))]
    public static void MeetingStarted() => JesterExileRuntime.State.BeginMeeting();

    // VotingComplete is already the host's synchronized, final result (including overrules).
    [HarmonyPrefix, HarmonyPriority(Priority.First), HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    public static void Capture(MeetingHud __instance, NetworkedPlayerInfo exiled, bool tie, ushort overruleNonce)
    {
        JesterExileRuntime.State.Capture(__instance.GetInstanceID(), overruleNonce,
            exiled ? exiled.PlayerId : null, exiled && exiled.Role ? (int)exiled.Role.Role : -1,
            exiled && exiled.Role is JesterRole, exiled && !exiled.IsDead && !exiled.Disconnected,
            tie, OptionGroupSingleton<JesterOptions>.Instance.JestWin == JestWinOptions.EndsGame);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class JesterConfirmedExilePatch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerControl __instance)
    {
        // Exiled() is also used by ghost catching and chain deaths; those are not a ballot victory.
        var voted = ExileController.Instance?.initData?.networkedPlayer;
        if (voted != null && voted && voted.PlayerId == __instance.PlayerId) JesterExileRuntime.State.Confirm(__instance.PlayerId);
    }
}
