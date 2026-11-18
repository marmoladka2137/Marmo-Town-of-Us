using System.Collections;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

public sealed class CompostTrap : IDisposable
{
    private static readonly List<CompostTrap> Traps = [];

    private readonly byte _ownerId;
    private readonly int _trapId;
    private Transform? _transform;
    private bool _triggered;

    private CompostTrap(PlayerControl owner, int trapId, Vector2 position)
    {
        _ownerId = owner.PlayerId;
        _trapId = trapId;

        var trapObject = new GameObject($"CompostTrap_{_ownerId}_{_trapId}");
        trapObject.transform.position = new Vector3(position.x, position.y, owner.transform.position.z + 0.001f);
        _transform = trapObject.transform;

        if (OptionGroupSingleton<ComposterOptions>.Instance.SeesTrapMarkers)
        {
            var renderer = trapObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ComposterAssets.TrapButton.LoadAsset();
            renderer.color = new Color(1f, 1f, 1f, 0.28f);
            renderer.sortingOrder = 4;
            trapObject.transform.localScale = Vector3.one * 0.32f;
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
        yield return new WaitForSeconds(OptionGroupSingleton<ComposterOptions>.Instance.ArmingDelay);

        while (_transform != null)
        {
            Update();
            yield return null;
        }
    }

    private void Update()
    {
        if (_triggered || _transform == null || !PlayerControl.LocalPlayer ||
            !PlayerControl.LocalPlayer.Data || !ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !TutorialManager.InstanceExists))
        {
            return;
        }

        var owner = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == _ownerId);
        if (owner == null || !owner.Data || owner.Data.Disconnected || owner.Data.Role is not ComposterRole role ||
            !owner.AmOwner || (!OptionGroupSingleton<ComposterOptions>.Instance.TrapsWorkAfterDeath && owner.HasDied()))
        {
            return;
        }

        if (!role.ActiveTrapIds.Contains(_trapId))
        {
            Remove(_ownerId, _trapId);
            return;
        }

        var radius = OptionGroupSingleton<ComposterOptions>.Instance.TrapRadius * ShipStatus.Instance.MaxLightRadius;
        var target = Helpers.GetAlivePlayers().FirstOrDefault(x =>
            x.PlayerId != _ownerId && Vector2.Distance(_transform.position, x.GetTruePosition()) <= radius);

        if (target == null)
        {
            return;
        }

        _triggered = true;
        ComposterRole.RpcTriggerCompostTrap(owner, target, _trapId);
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
