using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modules.Plumber;

public static class PlumberNailRuntime
{
    public static readonly VentSpikeState State = new();
    private static readonly Dictionary<byte, float> ReadyAt = new();
    private static readonly Dictionary<byte, int> Placed = new();
    private static readonly Dictionary<byte, int> OccupiedVents = new();
    public static int MaxTrappedVents => (int)TouMarmo.Modules.AbilityLimits.Count(OptionGroupSingleton<PlumberOptions>.Instance.MaxTrappedVents);
    public static int HitsToKill => (int)OptionGroupSingleton<PlumberOptions>.Instance.HitsRequiredToKill;
    public static float TrapCooldown => OptionGroupSingleton<PlumberOptions>.Instance.TrapCooldown;
    public static bool IsTrapped(int id) => State.Traps.ContainsKey(id);
    public static int GetHits(byte id) => State.Hits.GetValueOrDefault(id);
    public static void Clear() { State.Clear(); ReadyAt.Clear(); Placed.Clear(); OccupiedVents.Clear(); }
    public static void ClearEntries() { State.ClearEntries(); OccupiedVents.Clear(); }
    public static void OnVentExit(PlayerControl player, int id) => State.Exit(player.PlayerId, id);

    public static void ObserveVent(PlayerControl player)
    {
        if (!MarmoSession.IsHost || !MarmoSession.InRound || !player || !player.Data) return;
        if (!player.inVent || !MarmoSession.Alive(player))
        {
            if (OccupiedVents.Remove(player.PlayerId, out var exited)) State.Exit(player.PlayerId, exited);
            return;
        }
        if (!ShipStatus.Instance) return;
        var position = player.GetTruePosition();
        var destination = ShipStatus.Instance.AllVents.Where(v => v &&
            Vector2.Distance(position, v.transform.position) <= .65f)
            .OrderBy(v => Vector2.Distance(position, v.transform.position)).FirstOrDefault();
        if (!destination) return;
        if (OccupiedVents.TryGetValue(player.PlayerId, out var previous))
        {
            if (previous == destination!.Id) return;
            State.Exit(player.PlayerId, previous);
        }
        OccupiedVents[player.PlayerId] = destination!.Id;
        if (IsTrapped(destination.Id)) RpcVentEntry(player, destination.Id);
    }

    public static bool InRange(PlayerControl player, Vent vent) => player && vent &&
        Vector2.Distance(player.GetTruePosition(), vent.transform.position) <= vent.UsableDistance + .1f &&
        // Match vanilla Vent.CanUse: vent/short-object geometry must not block
        // interacting with the vent itself. Ship walls still block placement.
        !PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false);

    [MethodRpc((uint)TownOfUsRpc.PlumberPlaceNail, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPlaceNail(PlayerControl plumber, int ventId)
    {
        if (!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(plumber) ||
            plumber.Data.Role is not PlumberRole || plumber.inVent || !plumber.moveable || MeetingHud.Instance ||
            plumber.GetModifiers<TownOfUs.Modifiers.DisabledModifier>().Any(m=>!m.CanUseAbilities)) return;
        var vent = Helpers.GetVentById(ventId);
        if (!vent || !InRange(plumber, vent!) || !State.CanPlace(ventId, plumber.PlayerId, MaxTrappedVents) ||
            Time.time < ReadyAt.GetValueOrDefault(plumber.PlayerId)) return;
        var extra = OptionGroupSingleton<PlumberOptions>.Instance.TaskUses
            ? plumber.Data.Tasks.ToArray().Count(task => task.Complete) : 0;
        if (Placed.GetValueOrDefault(plumber.PlayerId) >= (long)MaxTrappedVents + extra) return;
        RpcApplyNail(PlayerControl.LocalPlayer, plumber, ventId);
    }

    [MethodRpc(1145, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcApplyNail(PlayerControl host, PlayerControl plumber, int ventId) => ApplyNail(host, plumber, ventId);

    internal static void ApplyNail(PlayerControl host, PlayerControl plumber, int ventId)
    {
        if (!host.IsHost() || !State.Place(ventId, plumber.PlayerId, MaxTrappedVents)) return;
        ReadyAt[plumber.PlayerId] = Time.time + CustomButtonSingleton<PlumberNailButton>.Instance.Cooldown;
        Placed[plumber.PlayerId] = Placed.GetValueOrDefault(plumber.PlayerId) + 1;
        if (!plumber.AmOwner) return;
        var button = CustomButtonSingleton<PlumberNailButton>.Instance;
        button.SetUses(button.ZeroIsInfinite && button.MaxUses==0 ? 0 :
            (int)Math.Clamp((long)button.MaxUses + button.ExtraUses - Placed[plumber.PlayerId],0,int.MaxValue));
        button.SetTimer(button.Cooldown);
        var flush = CustomButtonSingleton<PlumberFlushButton>.Instance;
        flush.SetTimer(flush.Cooldown);
        MarmoSession.Notify("Nail dropped into vent.");
    }

    // The host observes EVERY player's native vent animation, not only its own entry.
    [MethodRpc((uint)TownOfUsRpc.PlumberVentEntry, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcVentEntry(PlayerControl player, int ventId)
    {
        if (!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(player) || MeetingHud.Instance || !IsTrapped(ventId)) return;
        RpcTriggerNail(PlayerControl.LocalPlayer, player, ventId);
    }

    [MethodRpc(1146, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcTriggerNail(PlayerControl host, PlayerControl player, int ventId) => TriggerNail(host, player, ventId);

    internal static void TriggerNail(PlayerControl host, PlayerControl player, int ventId)
    {
        if (!host.IsHost() || !State.Trigger(ventId, player.PlayerId, HitsToKill, out var owner, out var lethal)) return;
        if (!lethal || !MarmoSession.IsHost || !MarmoSession.Alive(player)) return;
        var plumber = MarmoSession.Player(owner);
        if (!plumber) return;
        // Attribute the attack to the trap owner so normal shields are evaluated.
        plumber!.RpcSpecialMurder(player, MeetingCheck.OutsideMeeting, isIndirect: true,
            ignoreShield: false, resetKillTimer: false, teleportMurderer: false,
            showKillAnim: true, playKillSound: true, causeOfDeath: "Got Impaled");
    }
}

