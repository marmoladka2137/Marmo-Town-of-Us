using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Networking;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TouMarmo.Modules;

/// <summary>A misfire exemption is committed only after the intended victim actually dies.</summary>
public static class ChanceMisfireRuntime
{
    private static readonly Dictionary<byte, byte> Pending = new();
    public static bool IsPending(PlayerControl source, PlayerControl target) =>
        Pending.TryGetValue(source.PlayerId, out var id) && id == target.PlayerId;
    public static void Clear() => Pending.Clear();

    public static bool Request(PlayerControl source, PlayerControl? target)
    {
        if (!target || source.GetModifier<OneMoreChanceModifier>() is not { Used: false }) return false;
        RpcRequest(source, target!);
        return true;
    }

    [MethodRpc(1142, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequest(PlayerControl source, PlayerControl target)
    {
        if (!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(source) || !MarmoSession.Alive(target) ||
            source == target || source.Data.Role is not (SheriffRole or OfficerRole) ||
            source.GetModifier<OneMoreChanceModifier>() is not { Used: false } || Pending.ContainsKey(source.PlayerId)) return;
        float distance = source.Data.Role.GetAbilityDistance();
        if (Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > distance + .15f ||
            PhysicsHelpers.AnythingBetween(source.Collider, source.Collider.bounds.center, target.GetTruePosition(), Constants.ShipAndAllObjectsMask, false)) return;
        RpcBegin(source, target);
        source.RpcSpecialMurder(target, MeetingCheck.OutsideMeeting, causeOfDeath: "null");
        Coroutines.Start(Expire(source, target));
    }

    [MethodRpc(1143, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBegin(PlayerControl source, PlayerControl target) => Pending[source.PlayerId] = target.PlayerId;

    [RegisterEvent(-500)]
    public static void Killed(AfterMurderEvent e)
    {
        if (!MarmoSession.IsHost || !e.Target.HasDied() || !IsPending(e.Source, e.Target)) return;
        if (!OneMoreChanceModifier.Consume(e.Source, e.Target)) return;
        if (e.Source.Data.Role is SheriffRole) SheriffRole.RpcSheriffMisfire(e.Source, true);
        else if (e.Source.Data.Role is OfficerRole officer)
        {
            OfficerRole.RpcOfficerMisfire(e.Source, true);
            OfficerRole.RpcOfficerSyncBullets(e.Source, 0, officer.TotalBullets, Math.Max(0, officer.LoadedBullets - 1));
        }
        // Keep the receipt until all AfterMurder handlers finished on every peer.
    }

    private static IEnumerator Expire(PlayerControl source, PlayerControl target)
    {
        yield return new WaitForSeconds(3f);
        if (source && target && IsPending(source, target)) RpcFinish(source, target);
    }

    [MethodRpc(1144, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFinish(PlayerControl source, PlayerControl target)
    {
        if (IsPending(source, target)) Pending.Remove(source.PlayerId);
    }
}
