using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Events.Vanilla.Player;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TouMarmo.Options;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;
namespace TouMarmo.Modules.Gravedigger;

public static class GravediggerRuntime
{
    public sealed class State
    {
        public readonly BurialLedger Ledger = new();
        public readonly Dictionary<byte, string> Names = new();
        public float ReadyAt;
    }
    private static readonly Dictionary<byte, State> states = new();
    private static readonly Dictionary<byte, ushort> deathRoles = new();
    private static readonly Dictionary<byte, string> deathNames = new();
    // A cleaned body cannot be claimed twice, even by concurrent requests.
    private static readonly HashSet<byte> removed = new();
    private static readonly Dictionary<DeadBody, ArrowBehaviour> arrows = new();
    private static GravediggerRoleMenu? menu;
    public static GravediggerOptions Options => OptionGroupSingleton<GravediggerOptions>.Instance;
    public static int Required => (int)Options.RequiredBodies.Value;
    public static State For(PlayerControl p)
    {
        if (!states.TryGetValue(p.PlayerId, out var s)) states[p.PlayerId] = s = new();
        return s;
    }
    public static bool Locked(PlayerControl p) => p && states.TryGetValue(p.PlayerId, out var s) && s.Ledger.Inherited;
    public static float Remaining(PlayerControl p) => Mathf.Max(0, For(p).ReadyAt - Time.time);
    public static bool Choosing(PlayerControl p) => MarmoSession.Alive(p) && p.Data.Role is GravediggerRole &&
        !Bandit.BanditRuntime.IsBorrower(p) && MeetingHud.Instance &&
        MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted &&
        For(p).Ledger.Count > 0 && !For(p).Ledger.Inherited;
    public static DeadBody? Body(byte id) => Object.FindObjectsOfType<DeadBody>()
        .FirstOrDefault(b => b && b.ParentId == id && b.gameObject.activeInHierarchy && !b.Reported && !Papetir.PapetirRuntime.IsPaper(b));
    public static bool CanBury(PlayerControl p, DeadBody? body) => MarmoSession.Alive(p) &&
        p.Data.Role is GravediggerRole && MarmoSession.InRound && p.moveable && !p.inVent &&
        !TimeLordRewindSystem.IsRewinding && !For(p).Ledger.Inherited && Remaining(p) <= 0 &&
        !p.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities) &&
        body && body!.gameObject.activeInHierarchy && !body.Reported && !Papetir.PapetirRuntime.IsPaper(body) && !removed.Contains(body.ParentId) &&
        (MarmoSession.Player(body.ParentId) is not { } victim || victim.HasDied()) &&
        !For(p).Ledger.Buried.ContainsKey(body.ParentId) &&
        !Necromancer.NecromancerRuntime.States.Values.Any(s => s.Active && s.BodyId == body.ParentId) &&
        Vector2.Distance(p.GetTruePosition(), body.TruePosition) <= 1.5f &&
        Furas.FurasRuntime.ClearPath(p.GetTruePosition(), body.TruePosition);

    [MethodRpc(4200, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBuryRequest(PlayerControl owner, byte bodyId)
    {
        if (!MarmoSession.IsHost) return;
        var body = Body(bodyId);
        if (!CanBury(owner, body)) return;
        if (!deathRoles.TryGetValue(bodyId, out var roleId))
        {
            var victim = MarmoSession.Player(bodyId);
            if (!victim || !victim!.Data) return;
            roleId = (ushort)victim.GetRoleWhenAlive().Role;
        }
        RpcBuried(PlayerControl.LocalPlayer, owner, bodyId, roleId);
    }
    [MethodRpc(4201, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBuried(PlayerControl host, PlayerControl owner, byte bodyId, ushort roleId)
    {
        if (!host || !host.IsHost() || !owner || !owner.Data || owner.Data.Role is not GravediggerRole ||
            !removed.Add(bodyId)) return;
        var state = For(owner);
        if (!state.Ledger.Bury(bodyId, roleId)) return;
        state.Names[bodyId] = deathNames.GetValueOrDefault(bodyId) ?? MarmoSession.Player(bodyId)?.Data?.PlayerName ?? $"Player {bodyId}";
        if (state.Ledger.Won(!owner.HasDied(), Required))
            ((GravediggerRole)owner.Data.Role).HasBurialWin = true;
        state.ReadyAt = Time.time + Options.BuryCooldown.Value;
        // Remove every local copy, including Time Lord's hidden instance.
        RemoveBodies();
        BigBrainRuntime.Record(owner, MarmoSession.Player(bodyId) ?? owner, false, (ushort)owner.Data.Role.Role);
        if (owner.AmOwner)
            MarmoSession.Notify($"Bodies buried: {state.Ledger.Count}/{Required}. Choose a buried role at a meeting, or keep burying to win.");
    }
    private static void RemoveBodies()
    {
        foreach (var body in Object.FindObjectsOfType<DeadBody>(true))
            if (body && removed.Contains(body.ParentId))
            {
                // JanitorClean notifies existing corpse-arrow/evidence subscribers.
                var ownerId = states.FirstOrDefault(pair => pair.Value.Ledger.Buried.ContainsKey(body.ParentId)).Key;
                var owner = MarmoSession.Player(ownerId);
                if (owner) MiraEventManager.InvokeEvent(new TouAbilityEvent(AbilityType.JanitorClean, owner!, body));
                body.Reported = true;
                body.gameObject.SetActive(false);
                CrimeSceneComponent.ClearCrimeScene(body);
                Object.Destroy(body.gameObject);
            }
    }
    [MethodRpc(4202, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcInheritRequest(PlayerControl owner, ushort roleId)
    {
        if (!MarmoSession.IsHost || !Choosing(owner) || !For(owner).Ledger.CanInherit(roleId, RoleId.Get<GravediggerRole>())) return;
        var role = RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(r => (ushort)r.Role == roleId && !r.IsDead);
        if (role == null) return;
        if (!GravediggerRoleSetup.CanAssign(owner, role))
        { RpcRejected(PlayerControl.LocalPlayer, owner); return; }
        RpcInherited(PlayerControl.LocalPlayer, owner, roleId);
    }
    [MethodRpc(4204, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRejected(PlayerControl host, PlayerControl owner)
    {
        if (host && host.IsHost() && owner && owner.AmOwner)
            MarmoSession.Notify("This role has no eligible living target. Choose another buried role; your inheritance has not been used.");
    }
    [MethodRpc(4203, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcInherited(PlayerControl host, PlayerControl owner, ushort roleId)
    {
        if (!host || !host.IsHost() || !owner || !owner.Data || owner.Data.Role is not GravediggerRole ||
            !For(owner).Ledger.Inherit(roleId, RoleId.Get<GravediggerRole>())) return;
        var previousButtons = GravediggerRoleSetup.CaptureLocal(owner);
        owner.ChangeRole(roleId);
        if (MarmoSession.IsHost) Coroutines.Start(GravediggerRoleSetup.Assign(owner, roleId));
        if (owner.AmOwner)
        {
            GravediggerRoleSetup.ActivateLocal(owner, previousButtons);
            MarmoSession.Notify($"You are now {owner.Data.Role.GetRoleName()}. Your new role's victory condition applies.");
        }
    }
    public static void OpenMenu()
    {
        var local = PlayerControl.LocalPlayer;
        if (!Choosing(local)) return;
        CloseMenu();
        var state = For(local);
        var choices = state.Ledger.Buried.Select(pair => new BuriedRoleEntry(
            RoleManager.Instance.GetRole((RoleTypes)pair.Value), state.Names.GetValueOrDefault(pair.Key, $"Player {pair.Key}")))
            .Where(e => e.Role && !e.Role.IsDead).ToArray();
        menu = GravediggerRoleMenu.CreateMenu();
        menu.Open(choices, entry =>
        {
            if (entry == null) return;
            if (entry.Role is GravediggerRole) { MarmoSession.Notify("You cannot inherit Gravedigger."); return; }
            CloseMenu(); RpcInheritRequest(local, (ushort)entry.Role.Role);
        });
    }
    private static void CloseMenu()
    {
        if (menu != null && menu.Component) menu.ForceClose();
        menu = null;
    }
    public static void CloseViews(PlayerControl p)
    {
        if (!p || !p.AmOwner) return;
        CloseMenu(); ClearArrows();
    }
    private static void ClearArrows()
    {
        foreach (var arrow in arrows.Values) if (arrow) Object.Destroy(arrow.gameObject);
        arrows.Clear();
    }
    public static void Tick()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !ShipStatus.Instance || LobbyBehaviour.Instance) return;
        RemoveBodies(); // Also handles corpses that arrived after the burial RPC.
        if (menu != null && !Choosing(local)) CloseMenu();
        if (!MarmoSession.InRound || !MarmoSession.Alive(local) || local.Data.Role is not GravediggerRole)
        { ClearArrows(); return; }
        var bodies = Object.FindObjectsOfType<DeadBody>().Where(b => b && b.gameObject.activeInHierarchy && !b.Reported && !removed.Contains(b.ParentId)).ToArray();
        foreach (var body in arrows.Keys.Where(b => !b || !bodies.Contains(b)).ToArray())
        { if (arrows[body]) Object.Destroy(arrows[body].gameObject); arrows.Remove(body); }
        foreach (var body in bodies)
        {
            if (!arrows.TryGetValue(body, out var arrow)) arrows[body] = arrow = MiscUtils.CreateArrow(local.transform, Color.white);
            arrow.target = body.TruePosition;
            arrow.Update(); // Chef's arrows are explicitly advanced each frame too.
        }
    }
    [RegisterEvent]
    public static void Murder(AfterMurderEvent e)
    {
        if (e.Target && e.Target.Data)
        {
            deathRoles[e.Target.PlayerId] = (ushort)e.Target.GetRoleWhenAlive().Role;
            deathNames[e.Target.PlayerId] = e.Target.Data.PlayerName;
        }
    }
    [RegisterEvent]
    public static void Revive(TownOfUs.Events.TouEvents.PlayerReviveEvent e)
    {
        // A later death is a new physical corpse; do not silently erase it after revival.
        removed.Remove(e.Player.PlayerId);
        deathRoles.Remove(e.Player.PlayerId);
    }
    [RegisterEvent]
    public static void End(GameEndEvent e) { CloseMenu(); ClearArrows(); }
    [RegisterEvent(-1000)]
    public static void Round(RoundStartEvent e) { if (e.TriggeredByIntro) Clear(); }
    public static void Clear() { CloseMenu(); ClearArrows(); states.Clear(); deathRoles.Clear(); deathNames.Clear(); removed.Clear(); }
}

[HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), nameof(TownOfUs.Utilities.Extensions.ChangeRole))]
public static class GravediggerPermanentRolePatch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static bool Prefix(PlayerControl player, ushort newRoleType)
    {
        if (!GravediggerRuntime.Locked(player) || !MarmoSession.Alive(player)) return true;
        // One Gravedigger choice only. Intrinsic new-role transformations still work.
        return newRoleType != RoleId.Get<GravediggerRole>();
    }
}
