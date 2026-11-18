using System.Collections;
using TouMarmo.Assets;
using TouMarmo.Options;
using TouMarmo.Roles;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Composter;

public sealed class CompostTrap : IDisposable
{
    private static readonly List<CompostTrap> Traps = [];

    private readonly byte _ownerId;
    private readonly int _trapId;
    private Transform? _transform;
    private bool _triggered;
    private SpriteRenderer? _marker;

    private CompostTrap(PlayerControl owner, int trapId, Vector2 position)
    {
        _ownerId = owner.PlayerId;
        _trapId = trapId;

        var trapObject = new GameObject($"ComposterTrap_{_ownerId}_{_trapId}");
        trapObject.transform.position = new Vector3(position.x, position.y, owner.transform.position.z + 0.001f);
        _transform = trapObject.transform;

        // The marker exists only on the Composter's own client. Victims never receive a visible trap object.
        if (OptionGroupSingleton<ComposterOptions>.Instance.ShowOwnerMarker.Value)
        {
            var renderer = trapObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ComposterAssets.TrapFloor.LoadAsset();
            _marker = renderer;
            renderer.color = new Color(.8f, .85f, .8f, .45f);
            // Keep map-floor ordering; the world Z puts the marker below players, above the floor.
            renderer.sortingOrder = 0;
            trapObject.transform.localScale = Vector3.one;
        }
    }

    public static void Create(PlayerControl owner, int trapId, Vector2 position)
    {
        var trap = new CompostTrap(owner, trapId, position);
        Traps.Add(trap);
        Coroutines.Start(trap.CoRun());
    }

    public static void Remove(byte ownerId, int trapId)
    {
        var trap = Traps.FirstOrDefault(x => x._ownerId == ownerId && x._trapId == trapId);
        if (trap == null)
        {
            return;
        }

        trap.Dispose();
        Traps.Remove(trap);
    }

    public static void ClearFor(byte ownerId)
    {
        Traps.Where(x => x._ownerId == ownerId).ToList().Do(x => x.Dispose());
        Traps.RemoveAll(x => x._ownerId == ownerId);
    }

    private IEnumerator CoRun()
    {
        yield return new WaitForSeconds(OptionGroupSingleton<ComposterOptions>.Instance.ArmingDelay.Value);

        if (_marker) _marker!.color = new Color(1,1,1,0.85f);

        while (_transform != null)
        {
            UpdateTrap();
            yield return null;
        }
    }

    private void UpdateTrap()
    {
        if (_triggered || _transform == null || !PlayerControl.LocalPlayer ||
            !PlayerControl.LocalPlayer.Data || !ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !TutorialManager.InstanceExists))
        {
            return;
        }

        var owner = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == _ownerId);
        if (owner == null || !owner.Data || owner.Data.Disconnected || owner.Data.Role is not ComposterRole role ||
            !owner.AmOwner || (!OptionGroupSingleton<ComposterOptions>.Instance.WorkAfterDeath.Value && owner.HasDied()))
        {
            return;
        }

        if (!role.ActiveTrapIds.Contains(_trapId))
        {
            Remove(_ownerId, _trapId);
            return;
        }

        var radius = OptionGroupSingleton<ComposterOptions>.Instance.TrapRadius.Value * ShipStatus.Instance.MaxLightRadius;
        var target = Helpers.GetAlivePlayers().FirstOrDefault(x =>
            x.PlayerId != _ownerId && !x.inVent && Vector2.Distance(_transform.position, x.GetTruePosition()) <= radius && FurasRuntime.ClearPath(_transform.position,x.GetTruePosition()));

        if (target == null)
        {
            return;
        }

        _triggered = true;
        ComposterRole.RpcTriggerTrap(owner, target, _trapId);
    }

    public void Dispose()
    {
        if (_transform != null && _transform.gameObject != null)
        {
            Object.Destroy(_transform.gameObject);
        }

        _transform = null;
        GC.SuppressFinalize(this);
    }
}
