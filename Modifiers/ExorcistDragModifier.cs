using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TouMarmo.Buttons;
using TouMarmo.Options;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class ExorcistDragModifier(byte bodyId) : BaseModifier
{
    public override string ModifierName => "Drag";
    public override bool HideOnUi => true;
    public byte BodyId { get; } = bodyId;
    private DeadBody? deadBody;
    public DeadBody? DeadBody => deadBody ? deadBody : deadBody = Helpers.GetBodyById(BodyId);

    // Exorcist cannot vent while dragging (crew role, no venting).
    public override bool? CanVent() => false;

    public override void OnActivate()
    {
        // No special speed scaling; Exorcist drags at normal speed.
    }

    public override void OnDeactivate()
    {
        ExorcistRuntime.ReleaseBody(BodyId);
        if (Player && Player.AmOwner)
        {
            try { CustomButtonSingleton<ExorcistDragDropButton>.Instance.SetDrag(); } catch { }
        }

        if (DeadBody != null)
        {
            var touAbilityEvent = new TouAbilityEvent(AbilityType.UndertakerDrop, Player, DeadBody);
            MiraEventManager.InvokeEvent(touAbilityEvent);
            var dropPos = DeadBody.transform.position;
            dropPos.z = dropPos.y / 1000f;
            DeadBody.transform.position = dropPos;
        }
    }

    public override void OnMeetingStart()
    {
        if (Player) Player.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        if (Player) Player.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        if (!Player) return;
        if (!MarmoSession.Alive(Player) || Player.Data.Role is not ExorcistRole || !MarmoSession.InRound) { Player.RemoveModifier(this); return; }
        if (DeadBody == null || !DeadBody.myCollider || !DeadBody.myCollider.enabled)
        {
            Player.RemoveModifier(this);
            return;
        }
        var targetPos = Player.transform.position;
        targetPos.z = targetPos.y / 1000f;

        if (Player.inVent)
        {
            DeadBody.transform.position = targetPos;
        }
        else
        {
            DeadBody.transform.position = Vector3.Lerp(DeadBody.transform.position, targetPos, 5f * Time.deltaTime);
        }
    }
}
