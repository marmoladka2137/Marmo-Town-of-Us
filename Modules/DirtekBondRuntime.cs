using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Networking;
using TownOfUs.Events.TouEvents;

namespace TouMarmo.Modules;

public static class DirtekBondRuntime
{
    // Keep the bond through the player's transition to a ghost role.
    public static readonly Dictionary<byte, byte> Bonds = new();
    private static readonly HashSet<byte> Broken = new();
    private static readonly Dictionary<int, byte> Owners = new();

    public static void Bind(PlayerControl guard, PlayerControl target)
    {
        Bonds[guard.PlayerId] = target.PlayerId;
        Owners[guard.OwnerId] = guard.PlayerId;
    }

    public static void Break(PlayerControl guard, bool exile = false)
    {
        if (guard.Data?.Role is SoulguardRole role && role.ProtectedPlayer is { } target) Bind(guard, target);
        Break(guard.PlayerId, exile);
    }

    private static void Break(byte guardId, bool exile)
    {
        if (!MarmoSession.IsHost || !Bonds.TryGetValue(guardId, out var id) || Broken.Contains(guardId)) return;
        var target = MarmoSession.Player(id);
        if (!MarmoSession.Alive(target)) { Broken.Add(guardId); return; }
        // Serialize the connected host, not a character which may already have despawned.
        RpcBreakBond(PlayerControl.LocalPlayer, guardId, target!, exile);
    }

    [MethodRpc(1141, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBreakBond(PlayerControl sender, byte guardId, PlayerControl target, bool exile)
    {
        if (LobbyBehaviour.Instance || !ShipStatus.Instance || !Broken.Add(guardId)) return;
        ProtectionRuntime.BondDeaths.Add(target.PlayerId);
        var guard = MarmoSession.Player(guardId);
        if (guard && guard!.Data?.Role is SoulguardRole role) role.BondBreaking = true;
        if (exile)
        {
            // Exiled() is local; each recipient processes exactly one death, including votes.
            if (!target.HasDied()) target.Exiled();
        }
        else if (MarmoSession.IsHost)
        {
            var source = guard && guard!.Data != null && !guard.Data.Disconnected ? guard : target;
            source!.RpcSpecialMurder(target, MeetingCheck.Ignore, isIndirect: true, ignoreShield: true,
                resetKillTimer: false, createDeadBody: !MeetingHud.Instance,
                teleportMurderer: false, showKillAnim: false, causeOfDeath: "SoulguardBond");
        }
    }

    [RegisterEvent(350)]
    public static void Death(PlayerDeathEvent e) => Break(e.Player, e.DeathReason == DeathReason.Exile);

    [RegisterEvent]
    public static void Leave(PlayerLeaveEvent e)
    {
        if (Owners.TryGetValue(e.ClientData.Id, out var guardId)) Break(guardId, false);
        else if (e.ClientData.Character) Break(e.ClientData.Character);
    }

    [RegisterEvent]
    public static void Revive(PlayerReviveEvent e)
    {
        ProtectionRuntime.BondDeaths.Remove(e.Player.PlayerId);
        Broken.Remove(e.Player.PlayerId);
        if (e.Player.Data?.Role is SoulguardRole role)
        {
            role.BondBreaking = false;
            if (Bonds.TryGetValue(e.Player.PlayerId, out var targetId)) role.ProtectedPlayerId = targetId;
        }
    }

    [RegisterEvent]
    public static void RoleChanged(ChangeRoleEvent e)
    {
        // A living role change (e.g. Bandit) transfers the ability rather than breaking its bond.
        // Ghost conversion after a real death must retain the recorded bond until it is resolved.
        if (MarmoSession.Alive(e.Player) && e.Player.Data.Role is not SoulguardRole) Remove(e.Player.PlayerId);
    }

    private static void Remove(byte id)
    {
        Bonds.Remove(id);
        foreach (var entry in Owners.Where(x => x.Value == id).ToArray()) Owners.Remove(entry.Key);
    }

    public static void Tick()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
            if (MarmoSession.Alive(player) && player.Data.Role is SoulguardRole role)
            {
                if (role.ProtectedPlayerId == byte.MaxValue && Bonds.TryGetValue(player.PlayerId, out var targetId))
                    role.ProtectedPlayerId = targetId;
                if (role.ProtectedPlayer is { } target) Bind(player, target);
            }
        if (!MarmoSession.IsHost) return;
        foreach (var id in Bonds.Keys.ToArray())
            if (!Broken.Contains(id) && !MarmoSession.Alive(MarmoSession.Player(id))) Break(id, false);
    }

    public static void Clear() { Bonds.Clear(); Owners.Clear(); Broken.Clear(); }
}

// Vanilla shields also have a native field checked inside MurderPlayer independently of events.
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class DirtekNativeMurderBypass
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerControl target, ref MurderResultFlags resultFlags, out int __state)
    {
        __state = int.MinValue;
        if (!target || !ProtectionRuntime.Bypass(target)) return;
        __state = target.protectedByGuardianId;
        target.protectedByGuardianId = -1;
        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) && !resultFlags.HasFlag(MurderResultFlags.FailedError))
            resultFlags = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;
    }

    [HarmonyFinalizer]
    public static void Finalizer(PlayerControl target, int __state)
    {
        if (target && __state >= 0 && !target.HasDied()) target.protectedByGuardianId = __state;
    }
}
