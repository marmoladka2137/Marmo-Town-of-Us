using System.Collections;
using TouMarmo.Modules;
using TouMarmo.Options;
using TouMarmo.Roles;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Events;

public static class BarszczEvents
{
    private static readonly BepInEx.Logging.ManualLogSource Log =
        BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.Barszcz");

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not BarszczRole)
        {
            return;
        }

        Log.LogInfo($"Barszcz murder registered: killer={@event.Source.PlayerId}, victim={@event.Target.PlayerId}");

        var options = OptionGroupSingleton<BarszczOptions>.Instance;
        if (options.RevealBody.Value)
        {
            Coroutines.Start(CoCreateBodyArrow(@event.Target));
        }

        if (!@event.Source.AmOwner || !options.SwapPositions.Value)
        {
            return;
        }

        var sourcePosition = @event.Source.GetTruePosition();
        var farthest = PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player != @event.Source && player.Data != null &&
                             !player.Data.Disconnected && !player.HasDied())
            .OrderByDescending(player => Vector2.Distance(sourcePosition, player.GetTruePosition()))
            .FirstOrDefault();

        if (farthest == null)
        {
            return;
        }

        RpcSwapPositions(@event.Source, farthest, sourcePosition, farthest.GetTruePosition());
    }

    private static IEnumerator CoCreateBodyArrow(PlayerControl target)
    {
        DeadBody? deadBody = null;
        var elapsed = 0f;

        while (deadBody == null && elapsed < 2f)
        {
            deadBody = Object.FindObjectsOfType<DeadBody>()
                .FirstOrDefault(body => body.ParentId == target.PlayerId);
            if (deadBody != null)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (deadBody == null || PlayerControl.LocalPlayer == null)
        {
            yield break;
        }

        var arrow = MiscUtils.CreateArrow(PlayerControl.LocalPlayer.transform, BarszczColors.Role);
        while (deadBody != null && deadBody.gameObject != null && deadBody.gameObject.activeSelf &&
               MeetingHud.Instance == null && arrow != null && arrow.gameObject != null)
        {
            arrow.target = deadBody.transform.position;
            arrow.Update();
            yield return null;
        }

        if (arrow != null && arrow.gameObject != null)
        {
            Object.Destroy(arrow.gameObject);
        }
    }

    [MethodRpc((uint)ComposterRpc.SwapBarszczPositions, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSwapPositions(PlayerControl barszcz, PlayerControl farthest,
        Vector2 barszczPosition, Vector2 farthestPosition)
    {
        if (LobbyBehaviour.Instance || barszcz.Data.Role is not BarszczRole || barszcz.HasDied() ||
            farthest == barszcz || farthest.HasDied())
        {
            return;
        }

        if (barszcz.AmOwner)
        {
            barszcz.NetTransform.RpcSnapTo(farthestPosition);
        }

        if (farthest.AmOwner)
        {
            farthest.NetTransform.RpcSnapTo(barszczPosition);
        }
    }
}
