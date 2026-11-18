using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Patches;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TouMarmo.Assets;
using TouMarmo.Modifiers;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Networking;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
namespace TouMarmo.Modules.Furas;

[RegisterInIl2Cpp]
public sealed class FurasPetView(IntPtr ptr) : MonoBehaviour(ptr)
{
    internal PlayerControl? Controller;
    private void FixedUpdate()
    {
        if (Controller) FurasRuntime.MoveFox(Controller!);
    }
}

public sealed class FurasState
{
    public readonly List<Vector2> Points = new();
    public readonly List<GameObject> Markers = new();
    public int Selected;
    public bool Lost;
    public bool Active;
    public float EndsAt, ReadyAt, KillReadyAt, PlaceReadyAt, NextSync;
    public int Kills;
    public byte PendingVictim = byte.MaxValue;
    public bool PendingCrew;
    public bool AttackPending;
    public int ControlId, AttackId, ResolvedAttackId;
    public float RequestUntil;
    public FurasPetView? View;
    public FurasPetAnimation? Animation;
    public Vector2 RenderedPosition, InterpolationStart;
    public float LastPacketAt;
    public bool FacingLeft;
    public Vector2 Position;
    public bool PreviousMoveable;
    public Rigidbody2D? Body;
    public FurasMotion? Motion;
    public float Speed, DrivenSeconds, DistanceWalked;
}

public static class FurasRuntime
{
    private static readonly Dictionary<byte, FurasState> States = new();
    public static FurasOptions Options => OptionGroupSingleton<FurasOptions>.Instance;
    public static FurasState For(PlayerControl owner)
    {
        if (!States.TryGetValue(owner.PlayerId, out var state)) States[owner.PlayerId] = state = new();
        return state;
    }
    public static bool InRound => !LobbyBehaviour.Instance && !MeetingHud.Instance && !ExileController.Instance && !IntroCutscene.Instance && ShipStatus.Instance;
    public static bool CommsBlocked => Options.BlockedByComms.Value && (VanillaSystemCheckPatches.HqCommsSystem?.IsActive == true || VanillaSystemCheckPatches.HudCommsSystem?.IsActive == true);
    public static bool Available(PlayerControl p) => MarmoSession.InRound && p && p.Data != null && !p.Data.Disconnected && !p.HasDied() && p.Data.Role is FurasRole && !For(p).Lost;
    public static void Clear()
    {
        foreach (var id in States.Keys.ToArray()) Reset(id);
        sight = null;
    }
    public static void Reset(byte id)
    {
        if (!States.TryGetValue(id, out var s)) return;
        var owner = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == id);
        if (owner) Stop(owner!, s);
        else
        {
            s.Animation?.Dispose();
            if (s.View) Object.Destroy(s.View!.gameObject);
        }
        foreach (var marker in s.Markers) if (marker) Object.Destroy(marker);
        States.Remove(id);
    }
    public static void Stop(PlayerControl owner, FurasState s)
    {
        if (s.Active && owner.AmOwner)
        {
            owner.moveable = s.PreviousMoveable && !MeetingHud.Instance && !ExileController.Instance;
            owner.NetTransform.SetPaused(false);
            try
            {
                if (HudManager.Instance) HudManager.Instance.PlayerCam.SetTarget(owner);
                if (owner.lightSource)
                {
                    owner.lightSource.transform.SetParent(owner.transform, false);
                    owner.lightSource.Initialize(owner.Collider.offset / 2f);
                }
            }
            catch (Exception error) { Error($"Furas camera reset failed: {error}"); }
        }
        if (s.Active) s.ReadyAt = Time.time + Options.ControlCooldown.Value;
        if (s.Active && owner.AmOwner) Info($"Furas released: {s.DistanceWalked:F2} units over {s.DrivenSeconds:F2}s of movement input (including walls), configured speed {s.Speed:F2}.");
        s.Active = false;
        s.AttackPending = false;
        s.PendingVictim = byte.MaxValue;
        s.RequestUntil = 0;
        s.Body = null;
        s.Motion = null;
        s.Animation?.Dispose();
        s.Animation = null;
        if (s.View) Object.Destroy(s.View!.gameObject);
        s.View = null;
    }
    private static FurasMotion? sight;
    public static bool ClearPath(Vector2 from, Vector2 to) =>
        (sight ??= new FurasMotion(Constants.ShipAndObjectsMask)).ClearPath(from, to);
    public static void BeginControl(PlayerControl owner, int index)
    {
        // Ask the same movement routine that drives the crewmate. Sample and
        // restore velocity in one call, before freezing the body or stepping physics.
        var physics = owner.MyPhysics;
        var previous = physics.body.velocity;
        float walkingSpeed;
        try
        {
            physics.SetNormalizedVelocity(Vector2.right);
            walkingSpeed = physics.body.velocity.magnitude;
        }
        finally { physics.body.velocity = previous; }
        if (!(walkingSpeed > 0f) || !float.IsFinite(walkingSpeed))
        {
            MarmoSession.Notify("Unable to move right now.");
            return;
        }
        RpcControl(owner, index, walkingSpeed * Options.SpeedMultiplier.Value, For(owner).ControlId + 1);
    }
    internal static void MoveFox(PlayerControl owner)
    {
        if (!States.TryGetValue(owner.PlayerId, out var s) || !s.Active || !s.Body || s.Motion == null || !MarmoSession.InRound || owner.HasDied()) return;
        var input = CanDrive(owner) ? ReadPetDirection() : Vector2.zero;
        var position = s.Body!.position;
        var next = s.Motion.Step(position, input, s.Speed, Time.fixedDeltaTime);
        s.Body.MovePosition(next);
        s.Position = next;
        if (input.sqrMagnitude > 0f) s.DrivenSeconds += Time.fixedDeltaTime;
        s.DistanceWalked += Vector2.Distance(position, next);
    }
    internal static Vector2 ReadPetDirection()
    {
        if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
        {
            try
            {
                var x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) -
                        (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                var y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) -
                        (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                var normal = new Vector2(x, y);
                if (normal != Vector2.zero) return normal.normalized;
            }
            catch (Exception error) { Error($"Furas keyboard input unavailable: {error}"); }
        }
        var direction = AdvancedMovementUtilities.GetRegularDirection();
        return direction != Vector2.zero ? direction : AdvancedMovementUtilities.GetControllerPrimaryDirection();
    }
    public static PlayerControl? Target(PlayerControl owner)
    {
        var s = For(owner);
        if (!s.Active) return null;
        return PlayerControl.AllPlayerControls.ToArray().Where(p => p != owner && p.Data != null && !p.HasDied() && !p.Data.Disconnected && !p.inVent &&
            Vector2.Distance(s.Position, p.GetTruePosition()) <= Options.KillRange.Value && ClearPath(s.Position, p.GetTruePosition()))
            .OrderBy(p => Vector2.Distance(s.Position, p.GetTruePosition())).FirstOrDefault();
    }
    [MethodRpc(1020, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcPlace(PlayerControl owner, Vector2 position)
    {
        if (!Available(owner)) return;
        var s = For(owner);
        if (s.Active || s.Points.Count >= Options.MaxPoints.Value || Time.time < s.PlaceReadyAt || Vector2.Distance(position, owner.GetTruePosition()) > 0.6f) return;
        s.Points.Add(position);
        s.Selected = s.Points.Count - 1;
        s.PlaceReadyAt = Time.time + Options.PlaceCooldown.Value;
        if (!owner.AmOwner || !Options.ShowMarkers.Value) return;
        var marker = new GameObject("FurasSpawnPoint");
        marker.transform.position = new Vector3(position.x, position.y, position.y / 1000f + 0.01f);
        marker.transform.localScale = Vector3.one * 0.4f;
        var r = marker.AddComponent<SpriteRenderer>();
        r.sprite = FurasAssets.Paw.LoadAsset();
        r.color = new Color(1, 1, 1, 0.4f);
        s.Markers.Add(marker);
    }
    [MethodRpc(1021, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcControl(PlayerControl owner, int index, float speed, int controlId)
    {
        if (!Available(owner)) return;
        var s = For(owner);
        if (controlId <= s.ControlId || !float.IsFinite(speed) || speed <= 0 || CommsBlocked || s.Active || Time.time < s.ReadyAt || index < 0 || index >= s.Points.Count) return;
        s.ControlId = controlId;
        s.AttackId = s.ResolvedAttackId = 0;
        s.AttackPending = false; s.PendingVictim = byte.MaxValue; s.RequestUntil = 0;
        s.Position = s.Points[index];
        s.RenderedPosition = s.InterpolationStart = s.Position;
        s.LastPacketAt = Time.time;
        s.FacingLeft = false;
        s.Speed = speed;
        s.NextSync = s.DrivenSeconds = s.DistanceWalked = 0;
        s.EndsAt = Time.time + Options.ControlDuration.Value;
        s.KillReadyAt = Time.time + Options.FirstKillDelay.Value;
        s.Kills = 0;
        var obj = new GameObject("FurasPet");
        try
        {
            s.View = obj.AddComponent<FurasPetView>();
            s.Animation = new FurasPetAnimation(obj.transform, FurasAssets.Pet.LoadAsset(), owner.AmOwner || Options.VisiblePet.Value);
        }
        catch (Exception error)
        {
            s.Animation?.Dispose();
            s.Animation = null;
            s.View = null;
            Object.Destroy(obj);
            Error($"Furas pet creation failed: {error}");
            return;
        }
        var radius=Mathf.Clamp(owner.Collider.bounds.extents.x,0.20f,0.35f);
        s.Motion=new FurasMotion(Constants.ShipAndObjectsMask,radius);
        s.Active = true;
        obj.transform.position = new Vector3(s.Position.x, s.Position.y, s.Position.y / 1000f);
        if (!owner.AmOwner) return;
        s.PreviousMoveable = owner.moveable;
        try
        {
            owner.moveable = false;
            owner.MyPhysics.body.velocity = Vector2.zero;
            s.Body = obj.AddComponent<Rigidbody2D>();
            s.Body.bodyType = RigidbodyType2D.Kinematic;
            s.Body.position = s.Position;
            s.Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            s.View!.Controller = owner;
            owner.NetTransform.Halt();
            owner.NetTransform.SetPaused(true);
            MiscUtils.UpdateLocalPlayerCamera(s.View!, obj.transform);
        }
        catch (Exception error)
        {
            Stop(owner, s);
            Error($"Furas pet control failed: {error}");
        }
    }
    [MethodRpc(1022, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcMove(PlayerControl owner, Vector2 position, int controlId)
    {
        var s = For(owner);
        if (s.ControlId != controlId || !s.Active || !Available(owner) || !float.IsFinite(position.x) || !float.IsFinite(position.y)) return;
        if (owner.AmOwner) return;
        var elapsed = Mathf.Max(0.1f, Time.time - s.LastPacketAt);
        if (Vector2.Distance(s.Position, position) > s.Speed * (elapsed + 0.25f) + 0.3f) return;
        s.InterpolationStart = s.RenderedPosition;
        s.LastPacketAt = Time.time;
        s.Position = position;
    }
    [MethodRpc(1023, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStop(PlayerControl owner, int controlId)
    {
        var s = For(owner);
        if (s.ControlId == controlId) Stop(owner, s);
    }

    public static bool CanDrive(PlayerControl owner) => Available(owner) && !CommsBlocked &&
        !TimeLordRewindSystem.IsRewinding && !owner.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities) &&
        !(HudManager.Instance && HudManager.Instance.Chat.IsOpenOrOpening) && !Minigame.Instance;

    public static void RequestAttack(PlayerControl owner, PlayerControl victim)
    {
        var s = For(owner);
        if (!CanDrive(owner) || !s.Active || s.AttackPending || Time.unscaledTime < s.RequestUntil) return;
        s.RequestUntil = Time.unscaledTime + 1.5f;
        RpcMove(owner, s.Position, s.ControlId);
        RpcAttack(owner, victim, s.ControlId, s.AttackId + 1);
    }

    [MethodRpc(1024, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAttack(PlayerControl owner, PlayerControl victim, int controlId, int attackId)
    {
        if (!MarmoSession.IsHost || !Available(owner) || CommsBlocked || TimeLordRewindSystem.IsRewinding || owner.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities)) return;
        var s = For(owner);
        if (s.ControlId != controlId || attackId <= s.AttackId || s.AttackPending || !s.Active || Time.time < s.KillReadyAt || s.Kills >= Options.MaxKills.Value || !MarmoSession.Alive(victim) || victim == owner || victim.inVent ||
            Vector2.Distance(s.Position, victim.GetTruePosition()) > Options.KillRange.Value + 0.15f || !ClearPath(s.Position, victim.GetTruePosition())) return;
        s.KillReadyAt = Time.time + Options.KillCooldown.Value;
        s.PendingVictim = victim.PlayerId;
        s.PendingCrew = victim.IsCrewmate();
        s.AttackId = attackId;
        RpcAttackPending(PlayerControl.LocalPlayer, owner, controlId, attackId, Options.KillCooldown.Value);
        owner.RpcSpecialMurder(victim, MeetingCheck.OutsideMeeting, isIndirect: true, ignoreShield: false,
            resetKillTimer: false, teleportMurderer: false, showKillAnim: true, causeOfDeath: "FurasPet");
        Coroutines.Start(FinishAttack(owner, victim, controlId, attackId));
    }
    [MethodRpc(1086, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAttackPending(PlayerControl host, PlayerControl owner, int controlId, int attackId, float cooldown)
    {
        if (!host || !host.IsHost() || !owner || !float.IsFinite(cooldown)) return;
        var s = For(owner);
        ApplyAttackPending(s, controlId, attackId, cooldown);
    }
    internal static bool ApplyAttackPending(FurasState s, int controlId, int attackId, float cooldown)
    {
        if (!s.Active || s.ControlId != controlId || attackId <= s.ResolvedAttackId || attackId < s.AttackId ||
            s.AttackPending && attackId == s.AttackId || !float.IsFinite(cooldown)) return false;
        s.AttackId = attackId; s.AttackPending = true;
        s.KillReadyAt = Time.time + Mathf.Clamp(cooldown, 0, Options.KillCooldown.Value);
        return true;
    }
    private static IEnumerator FinishAttack(PlayerControl owner, PlayerControl victim, int controlId, int attackId)
    {
        yield return null; yield return null;
        var s = For(owner);
        if (s.Active && s.ControlId == controlId && s.AttackPending && s.AttackId == attackId)
            ResolveAttack(owner, victim && victim.HasDied());
    }
    private static void ResolveAttack(PlayerControl owner, bool killed)
    {
        var s = For(owner);
        var lost = killed && s.PendingCrew && Options.LoseOnCrewKill.Value;
        var victim = MarmoSession.Player(s.PendingVictim);
        if (lost && victim && OneMoreChanceModifier.Consume(owner, victim!)) lost = false;
        RpcKillResult(PlayerControl.LocalPlayer, owner, s.ControlId, s.AttackId, killed, lost, Mathf.Max(0,s.KillReadyAt-Time.time));
    }
    [RegisterEvent]
    public static void Killed(AfterMurderEvent ev)
    {
        if (ev.Source.Data.Role is not FurasRole || !ev.Target.HasDied()) return;
        if (ev.DeadBody)
        {
            var mark = new GameObject("Furas paw prints");
            mark.transform.SetParent(ev.DeadBody!.transform,false);
            mark.transform.localPosition = new Vector3(0.25f,-0.1f,-0.01f);
            mark.transform.localScale = Vector3.one * 0.13f;
            var renderer = mark.AddComponent<SpriteRenderer>();
            renderer.sprite = FurasAssets.Paw.LoadAsset();
            renderer.color = new Color(0.55f,0.28f,0.12f,0.85f);
        }
        if (!MarmoSession.IsHost) return;
        var s = For(ev.Source);
        if (!s.Active || !s.AttackPending || s.PendingVictim != ev.Target.PlayerId) return;
        ResolveAttack(ev.Source, true);
    }
    [MethodRpc(1025, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcKillResult(PlayerControl host, PlayerControl owner, int controlId, int attackId, bool killed, bool lost, float cooldown)
    {
        if (!host || !host.IsHost() || !owner || !float.IsFinite(cooldown)) return;
        var s = For(owner);
        if (!ApplyAttackResult(s, controlId, attackId, killed, lost, cooldown)) return;

        // The host resolved and consumed One More Chance before sending this result.
        // Recipients must not re-read a modifier which a preceding RPC has already removed.
        if (killed && (lost || Options.ReturnAfterKill.Value)) Stop(owner, s);
        if (killed && lost && owner.AmOwner)
            Helpers.CreateAndShowNotification("Your pet killed a Crewmate. Your ability is permanently lost.", Color.white, new Vector3(0, 1, -20));
    }
    internal static bool ApplyAttackResult(FurasState s, int controlId, int attackId, bool killed, bool lost, float cooldown)
    {
        if (!s.Active || s.ControlId != controlId || attackId != s.AttackId || attackId <= s.ResolvedAttackId || !float.IsFinite(cooldown)) return false;
        s.ResolvedAttackId = attackId;
        s.AttackPending = false; s.PendingVictim = byte.MaxValue; s.RequestUntil = 0;
        if (killed) s.Kills++;
        s.KillReadyAt = Time.time + Mathf.Clamp(cooldown, 0, Options.KillCooldown.Value);

        s.Lost |= killed && lost;
        return true;
    }
    public static void Tick()
    {
        foreach (var entry in States.ToArray())
        {
            var owner = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == entry.Key);
            var s = entry.Value;
            if (!owner || owner!.Data == null || owner.Data.Disconnected || owner.Data.Role is not FurasRole || LobbyBehaviour.Instance)
            { Reset(entry.Key); continue; }
            if (!MarmoSession.InRound || owner.HasDied())
            {
                Stop(owner, s);
                if (MeetingHud.Instance && !Options.KeepPoints.Value)
                {
                    s.Points.Clear(); s.Selected = 0;
                    foreach (var marker in s.Markers) if (marker) Object.Destroy(marker);
                    s.Markers.Clear();
                }
                continue;
            }
            if (!s.Active) continue;
            if (!s.View || owner.AmOwner && (!s.Body || s.Motion == null))
            {
                Warning($"Furas control {s.ControlId} ended because its puppet was removed.");
                Stop(owner, s);
                continue;
            }
            if (Time.time >= s.EndsAt) { Stop(owner, s); continue; }
            if (owner.AmOwner)
            {
                owner.moveable = false;
                owner.MyPhysics.body.velocity = Vector2.zero;
                if (CommsBlocked || TimeLordRewindSystem.IsRewinding || owner.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities))
                { RpcStop(owner, s.ControlId); continue; }
                if (Time.time >= s.NextSync) { s.NextSync = Time.time + 0.1f; RpcMove(owner, s.Position, s.ControlId); }
            }
            var rendered = owner.AmOwner && s.View ? (Vector2)s.View!.transform.position : Vector2.Lerp(s.InterpolationStart, s.Position, Mathf.Clamp01((Time.time - s.LastPacketAt) / 0.1f));
            if (!owner.AmOwner && s.Motion != null)
            {
                var delta=rendered-s.RenderedPosition;
                if(delta.sqrMagnitude>0.000001f)
                    rendered=s.Motion.Step(s.RenderedPosition,delta.normalized,delta.magnitude,1f);
            }
            var movement = rendered - s.RenderedPosition;
            if (Mathf.Abs(movement.x) > 0.0001f) s.FacingLeft = movement.x < 0;
            s.RenderedPosition = rendered;
            s.Animation?.Tick(movement.magnitude, Time.deltaTime, s.FacingLeft);
            // Camera and light follow a stable root; only the puppet is animated.
            if (!owner.AmOwner && s.View) s.View!.transform.position = new Vector3(rendered.x, rendered.y, rendered.y / 1000f);
        }
    }
}
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class FurasUpdatePatch
{
    [HarmonyPostfix] public static void Postfix() => FurasRuntime.Tick();
}


