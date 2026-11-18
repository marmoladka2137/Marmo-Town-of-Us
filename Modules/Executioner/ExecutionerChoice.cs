using TownOfUs.Roles;
using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Options;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
namespace TouMarmo.Modules.Executioner;
public static class ExecutionerChoice
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.ExecutionerChoice");
    public static readonly HashSet<byte> Used = new();
    public static bool FirstMeetingStarted { get; private set; }
    public static bool ChoiceOpen(PlayerControl p) => p &&
        NewRoleRules.ExecutionerChoiceOpen(FirstMeetingStarted,Used.Contains(p.PlayerId));
    public static void Reset()
    {
        Used.Clear();
        FirstMeetingStarted=false;
    }
    [RegisterEvent(-1000)] public static void CloseFirstRound(StartMeetingEvent e) => FirstMeetingStarted=true;
    [RegisterEvent(-1000)] public static void RoundStarted(RoundStartEvent e)
    {
        if(e.TriggeredByIntro) Reset();
    }
    public static ExecutionerChoiceOptions Options => OptionGroupSingleton<ExecutionerChoiceOptions>.Instance;
    public static bool CanChoose(PlayerControl p) => MarmoSession.InRound && MarmoSession.Alive(p) && p.Data.Role is ExecutionerRole exe &&
        !exe.AboutToWin && !exe.MetWinCon && ChoiceOpen(p);
    private static PlayerControl[] Candidates(PlayerControl p, ExecutionerRole exe) => PlayerControl.AllPlayerControls.ToArray()
        .Where(t => t && t.Data != null && AuditRules.ExecutionerCandidate(
            !t.HasDied(), !t.Data.Disconnected, t.IsCrewmate(), t == p, t == exe.Target,
            t.GetModifiers<ExecutionerTargetModifier>().Any(m => m.OwnerId != p.PlayerId) ||
                t.HasModifier<GuardianAngelTargetModifier>() || t.HasModifier<AllianceGameModifier>(),
            SpectatorRole.TrackedSpectators.Contains(t.Data.PlayerName))).ToArray();
    public static bool HasNewTarget(PlayerControl p) => p.Data.Role is ExecutionerRole exe && Candidates(p, exe).Length > 0;
    internal static PlayerControl? PickRandomTarget(PlayerControl player, ExecutionerRole role)
    {
        var choices = Candidates(player, role);
        return choices.Length == 0 ? null : choices[System.Random.Shared.Next(choices.Length)];
    }
    [MethodRpc(1030, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequest(PlayerControl player, bool renounce)
    {
        if (!MarmoSession.IsHost) return;
        if (!CanChoose(player))
        {
            var roleName = player && player.Data && player.Data.Role ? player.Data.Role.GetType().Name : "<none>";
            var playerName = player && player.Data != null ? player.Data.PlayerName : "<null>";
            var used = player && Used.Contains(player.PlayerId);
            var open = player && ChoiceOpen(player);
            Log.LogWarning($"Executioner request ignored for {playerName} (renounce={renounce}, role={roleName}, inRound={FurasRuntime.InRound}, firstMeeting={FirstMeetingStarted}, used={used}, open={open}).");
            return;
        }
        if (renounce ? !Options.Renounce.Value : !Options.ChangeTarget.Value)
        {
            Log.LogWarning($"Executioner request ignored for {player?.Data?.PlayerName}: option disabled (renounce={renounce}, ChangeTarget={Options.ChangeTarget.Value}, Renounce={Options.Renounce.Value}).");
            return;
        }
        var exe = (ExecutionerRole)player.Data.Role;
        var candidates = Candidates(player, exe);
        if (!renounce && candidates.Length == 0)
        {
            Log.LogWarning($"Executioner new-target request ignored for {player.Data?.PlayerName}: no candidates.");
            return;
        }
        Log.LogInfo($"Executioner request accepted for {player.Data?.PlayerName} (renounce={renounce}, candidates={candidates.Length}).");
        var chosen = renounce ? player : PickRandomTarget(player, exe);
        if (!chosen) return;
        RpcApply(player, chosen!, renounce);
    }
    [MethodRpc(1031, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcApply(PlayerControl player, PlayerControl target, bool renounce)
    {
        try
        {
            if (!player || player.Data == null || player.Data.Role is not ExecutionerRole exe)
            {
                var playerName = player?.Data?.PlayerName ?? "<null>";
                var roleName = player?.Data?.Role?.GetType().Name ?? "<none>";
                Log.LogWarning($"Executioner apply skipped: invalid player/role (player={playerName}, role={roleName}, renounce={renounce}).");
                return;
            }
            if (Used.Contains(player.PlayerId))
            {
                Log.LogInfo($"Executioner apply skipped for {player.Data.PlayerName}: choice already used.");
                return;
            }
            // Candidate eligibility belongs to RpcRequest on the host. A client must apply
            // the accepted result even if its local role/death packets are in flight.
            if (!renounce && (!target || target.Data == null)) return;
            // Reserve before callbacks can re-enter or delayed native target assignment arrives.
            Used.Add(player.PlayerId);
            foreach (var p in PlayerControl.AllPlayerControls.ToArray())
            {
                var component = p ? p.GetModifierComponent() : null;
                if (component == null) continue;
                foreach (var marker in component.ActiveModifiers.ToArray().OfType<ExecutionerTargetModifier>().Where(m=>m.OwnerId==player.PlayerId).ToArray())
                    component.RemoveModifier(marker);
            }
            exe.Target = null;
            exe.Voters.Clear();
            exe.AboutToWin = false;
            exe.TargetVoted = false;
            exe.TargetVotedAsEvil = false;
            if (renounce)
            {
                player.ChangeRole(RoleId.Get<AmnesiacRole>());
                Log.LogInfo($"Executioner {player.Data.PlayerName} renounced and became Amnesiac.");
            }
            else
            {
                exe.Target = target;
                if (target) target.AddModifier<ExecutionerTargetModifier>(player.PlayerId);
                Log.LogInfo($"Executioner {player.Data?.PlayerName} received new target {(target && target.Data!=null ? target.Data.PlayerName : "<null>")}.");
            }
            TownOfUs.Utilities.TouRoleUtils.ClearTaskHeader(player);
            player.Data.Role.SpawnTaskHeader(player);
            if (player.AmOwner && HudManager.Instance && player.Data?.Role != null)
                HudManager.Instance.SetHudActive(player,player.Data.Role,true);
            if (player.AmOwner)
                Helpers.CreateAndShowNotification(renounce ? "You renounced Executioner and became Amnesiac." : $"Your new target is {(target && target.Data != null ? target.Data.PlayerName : "<null>")}. Your one-time choice is used.",
                    UnityEngine.Color.white, new UnityEngine.Vector3(0, 1, -20));
        }
        catch (System.Exception e)
        {
            Log.LogError($"Executioner apply failed for {(player ? player.Data?.PlayerName : "<null>")} (renounce={renounce}): {e}");
        }
    }
}
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
public static class ExecutionerChoiceReset
{
    [HarmonyPrefix] public static void Prefix() => ExecutionerChoice.Reset();
}

