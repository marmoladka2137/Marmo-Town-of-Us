using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TMPro;
using TownOfUs.Modifiers;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using TownOfUs.Networking;
using UnityEngine;
using Object = UnityEngine.Object;
namespace TouMarmo.Modules.Manipulator;

public sealed class ManipulatorObserverModifier : DisabledModifier
{
    public override string ModifierName => "Observing manipulation";
    public override bool AutoStart => false;
    public override float Duration => float.MaxValue;
}

public static class ManipulatorRuntime
{
    public sealed class Session(PlayerControl owner, PlayerControl target, int id, float duration)
    {
        public readonly PlayerControl Owner = owner, Target = target;
        public readonly int Id = id;
        public readonly ManipulationClock Clock = new(duration);
        public bool PreviousMoveable, Attached, Pending;
        public byte Victim = byte.MaxValue;
        public float RetryAt;
    }
    private static readonly Dictionary<byte, Session> sessions = new();
    private static readonly Dictionary<byte, float> ready = new();
    private static int nextId;
    private static SpriteRenderer? overlay;
    private static TextMeshPro? countdown;
    public static ManipulatorOptions Options => OptionGroupSingleton<ManipulatorOptions>.Instance;
    public static bool Observing(PlayerControl p) => p && sessions.ContainsKey(p.PlayerId);
    public static Session? VictimSession(PlayerControl p) => p ? sessions.Values.FirstOrDefault(s => s.Target == p && !s.Clock.Resolved) : null;
    public static float Cooldown(PlayerControl p) => p && ready.TryGetValue(p.PlayerId, out var at) ? Mathf.Max(0, at - Time.time) : 0;
    public static float Range => GameOptionsManager.Instance.currentNormalGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances)[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
    public static bool CanStart(PlayerControl p) => MarmoSession.Alive(p) && p.Data.Role is ManipulatorRole &&
        MarmoSession.InRound && sessions.Count == 0 && Cooldown(p) <= 0 && (p.AmOwner ? p.CanMove : p.moveable) && !p.inVent && !TimeLordRewindSystem.IsRewinding &&
        !p.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities);

    public static void RequestAfterMenu(PlayerControl target) => Coroutines.Start(WaitForMenu(target));
    private static System.Collections.IEnumerator WaitForMenu(PlayerControl target)
    {
        var owner=PlayerControl.LocalPlayer;
        var deadline=Time.unscaledTime+2f;
        // ForceClose can restore movement at the end of the frame. Sending the
        // host request inside the selection callback rejects the local host.
        yield return null;
        while(owner && MarmoSession.InRound && Time.unscaledTime<deadline && (Minigame.Instance || !owner.CanMove)) yield return null;
        if(!owner || !CanStart(owner) || !MarmoSession.Alive(target)) yield break;
        RpcRequest(owner,target);
    }

    [MethodRpc(4000, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRequest(PlayerControl owner, PlayerControl target)
    {
        if (!MarmoSession.IsHost || !CanStart(owner) || !MarmoSession.Alive(target) || target == owner ||
            target.IsInTargetingAnimState() || VictimSession(target) != null ||
            TownOfUs.Modules.ControlSystem.PuppeteerControlState.IsControlled(target.PlayerId, out _)) return;
        RpcBegin(PlayerControl.LocalPlayer, owner, target, ++nextId, Options.TimeToKill.Value);
    }

    [MethodRpc(4001, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBegin(PlayerControl host, PlayerControl owner, PlayerControl target, int id, float duration)
    {
        if (!host.IsHost() || !MarmoSession.Alive(owner) || !MarmoSession.Alive(target) || owner == target ||
            sessions.Count != 0 || !float.IsFinite(duration) || duration <= 0) return;
        var s = new Session(owner,target,id,duration);
        sessions.Add(owner.PlayerId,s);
        owner.AddModifier<ManipulatorObserverModifier>();
        if (owner.AmOwner)
        {
            if (Minigame.Instance) Minigame.Instance.Close();
            if (MapBehaviour.Instance) MapBehaviour.Instance.Close();
            s.PreviousMoveable=owner.moveable; s.Attached=true;
            owner.moveable=false; owner.MyPhysics.body.velocity=Vector2.zero;
            owner.NetTransform.Halt(); owner.NetTransform.SetPaused(true);
            Attach(s);
        }
        if (target.AmOwner)
        {
            if (Minigame.Instance) Minigame.Instance.Close();
            Helpers.CreateAndShowNotification("You are manipulated! Kill someone before time runs out.", Color.red, new Vector3(0,1,-20));
        }
        BigBrain.BigBrainRuntime.Record(owner,target,false);
    }

    private static void Attach(Session s)
    {
        if (HudManager.Instance) HudManager.Instance.PlayerCam.SetTarget(s.Target);
        if (s.Owner.lightSource)
        {
            s.Owner.lightSource.transform.SetParent(s.Target.transform,false);
            s.Owner.lightSource.Initialize(s.Target.Collider.offset/2f);
        }
    }

    public static bool CanAttack(PlayerControl p) => VictimSession(p) is { Pending:false } s &&
        s.Clock.Remaining > 0 &&
        MarmoSession.InRound && MarmoSession.Alive(p) && !p.inVent && p.CanMove &&
        !TimeLordRewindSystem.IsRewinding && Time.unscaledTime >= s.RetryAt &&
        !p.GetModifiers<DisabledModifier>().Any(m => !m.CanUseAbilities);
    public static PlayerControl? Target(PlayerControl p) => p.GetClosestLivingPlayer(true,Range,
        predicate: t => t != p && MarmoSession.Alive(t) && !t.inVent && !t.IsInTargetingAnimState());

    [MethodRpc(4002, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAttack(PlayerControl actor, PlayerControl victim, int id)
    {
        var s=VictimSession(actor);
        if (!MarmoSession.IsHost || s == null || s.Id != id || !CanAttack(actor) || !MarmoSession.Alive(victim) ||
            actor == victim || victim.inVent || victim.IsInTargetingAnimState() ||
            Vector2.Distance(actor.GetTruePosition(),victim.GetTruePosition()) > Range ||
            !Furas.FurasRuntime.ClearPath(actor.GetTruePosition(),victim.GetTruePosition())) return;
        s.Pending=true; s.Victim=victim.PlayerId;
        s.Owner.RpcFramedMurder(victim,actor, resetKillTimer:false, causeOfDeath:"Manipulator");
        Coroutines.Start(CheckAttack(s));
    }
    private static System.Collections.IEnumerator CheckAttack(Session s)
    {
        // Custom murder completes after its animation coroutine; two frames are
        // insufficient to decide that the attack failed.
        var until=Time.unscaledTime+3f;
        while(Time.unscaledTime<until && !s.Clock.Resolved) yield return null;
        if (!sessions.TryGetValue(s.Owner.PlayerId,out var current) || current != s || s.Clock.Resolved) yield break;
        s.Pending=false; s.Victim=byte.MaxValue; s.RetryAt=Time.unscaledTime+.5f;
    }
    [RegisterEvent]
    public static void Murder(AfterMurderEvent e)
    {
        if (!MarmoSession.IsHost || !e.Target.HasDied()) return;
        if (sessions.TryGetValue(e.Source.PlayerId,out var s) && s.Pending && s.Victim == e.Target.PlayerId)
            RpcResolve(PlayerControl.LocalPlayer,s.Owner,s.Id);
    }
    [MethodRpc(4003, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcResolve(PlayerControl host, PlayerControl owner, int id)
    {
        if (!host.IsHost() || !sessions.TryGetValue(owner.PlayerId,out var s) || s.Id != id) return;
        s.Clock.Resolve(); s.Pending=false;
    }
    [MethodRpc(4004, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcEnd(PlayerControl host, PlayerControl owner, int id)
    {
        if (host.IsHost() && sessions.TryGetValue(owner.PlayerId,out var s) && s.Id==id) Release(s);
    }
    public static void CancelFor(PlayerControl player)
    {
        foreach(var s in sessions.Values.ToArray()) if(s.Owner==player || s.Target==player) Release(s);
    }
    public static void Clear()
    {
        foreach(var s in sessions.Values.ToArray()) Release(s);
        ready.Clear(); ClearVisuals();
    }
    private static void Release(Session s)
    {
        sessions.Remove(s.Owner.PlayerId);
        if(s.Owner)
        {
            s.Owner.RemoveModifier<ManipulatorObserverModifier>();
            ready[s.Owner.PlayerId]=Time.time+Options.Cooldown.Value;
            if(s.Owner.AmOwner && s.Attached)
            {
                s.Owner.NetTransform.SetPaused(false);
                s.Owner.moveable=s.PreviousMoveable;
                if(HudManager.Instance) HudManager.Instance.PlayerCam.SetTarget(s.Owner);
                if(s.Owner.lightSource)
                {
                    s.Owner.lightSource.transform.SetParent(s.Owner.transform,false);
                    s.Owner.lightSource.Initialize(s.Owner.Collider.offset/2f);
                }
            }
        }
        ClearVisuals();
    }
    private static void ClearVisuals()
    {
        ManipulatorAudio.Stop();
        if(overlay) Object.Destroy(overlay!.gameObject);
        if(countdown) Object.Destroy(countdown!.gameObject);
        overlay=null; countdown=null;
    }
    [RegisterEvent]
    public static void Round(RoundStartEvent e)
    {
        if(!e.TriggeredByIntro) return;
        foreach(var s in sessions.Values.ToArray()) Release(s);
        ready.Clear(); nextId=0;
    }
    public static void Tick()
    {
        var meeting=MeetingHud.Instance || ExileController.Instance;
        foreach(var s in sessions.Values.ToArray())
        {
            if(!ShipStatus.Instance || LobbyBehaviour.Instance || !GameData.Instance || !MarmoSession.Alive(s.Owner) ||
                s.Owner.Data.Role is not ManipulatorRole || !s.Target || s.Target.Data.Disconnected)
            {Release(s);continue;}
            if(meeting && Options.MeetingCancels.Value) {Release(s);continue;}
            if(meeting)
            {
                if(s.Owner.AmOwner && HudManager.Instance) HudManager.Instance.PlayerCam.SetTarget(s.Owner);
                continue;
            }
            if(s.Owner.AmOwner)
            {
                s.Owner.moveable=false; s.Owner.MyPhysics.body.velocity=Vector2.zero;
                Attach(s);
            }
            if(MarmoSession.IsHost && !s.Clock.Resolved && !MarmoSession.Alive(s.Target))
                RpcResolve(PlayerControl.LocalPlayer,s.Owner,s.Id);
            var expired=s.Clock.Advance(Time.deltaTime, false);
            if(MarmoSession.IsHost && expired && !s.Pending)
            {
                // The deadline punishment is a self-death, not a second voluntary attack.
                RpcResolve(PlayerControl.LocalPlayer,s.Owner,s.Id);
                s.Target.RpcSelfMurder(s.Target,s.Target,ignoreShield:true,resetKillTimer:false,causeOfDeath:"Manipulator");
            }
            if(MarmoSession.IsHost && s.Clock.Finished) RpcEnd(PlayerControl.LocalPlayer,s.Owner,s.Id);
        }
        var local=PlayerControl.LocalPlayer;
        var victim=local ? VictimSession(local) : null;
        if(victim==null || meeting || !MarmoSession.Alive(local)) {ClearVisuals();return;}
        ManipulatorAudio.Update(victim.Clock.Remaining > 0);
        if(!HudManager.Instance) return;
        HudManager.Instance.KillButton.ToggleVisible(false);
        if(!overlay && HudManager.Instance.FullScreen)
        {
            overlay=Object.Instantiate(HudManager.Instance.FullScreen,HudManager.Instance.FullScreen.transform.parent);
            overlay.name="ManipulatorRedOverlay"; overlay.transform.localScale*=10f;
            overlay.gameObject.SetActive(true); overlay.enabled=true; overlay.color=new Color(1,0,0,.18f);
        }
        if(!countdown)
        {
            countdown=Object.Instantiate(HudManager.Instance.KillButton.cooldownTimerText,HudManager.Instance.transform);
            countdown.name="ManipulatorCountdown";
            countdown.transform.localPosition=new Vector3(0,1.6f,-30);
            countdown.transform.localScale=Vector3.one;
            countdown.rectTransform.sizeDelta=new Vector2(8f,1f);
            countdown.alignment=TextAlignmentOptions.Center;
            countdown.enableWordWrapping=false;
            countdown.fontSize=2.5f; countdown.color=Color.red;
            countdown.gameObject.SetActive(true);
        }
        countdown!.text=$"KILL OR DIE: {Mathf.CeilToInt(victim.Clock.Remaining)}";
        countdown.transform.localPosition=new Vector3(Mathf.Sin(Time.unscaledTime*61f)*.035f,1.6f+Mathf.Sin(Time.unscaledTime*83f)*.018f,-30);
    }
}

[HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
public static class ManipulatorHudPatch
{
    [HarmonyPostfix] public static void Postfix() => ManipulatorRuntime.Tick();
}
[HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.OnGameEnd))]
public static class ManipulatorEndPatch
{
    [HarmonyPrefix] public static void Prefix()
    {
        ManipulatorRuntime.Clear();
    }
}

[HarmonyPatch]
public static class ManipulatorExitPatch
{
    public static IEnumerable<System.Reflection.MethodBase> TargetMethods() => typeof(AmongUsClient)
        .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
        .Where(m=>m.Name=="ExitGame"&&!m.IsAbstract);
    [HarmonyPrefix] public static void Prefix() => ManipulatorRuntime.Clear();
}

[HarmonyPatch]
public static class ManipulatorButtonGate
{
    public static IEnumerable<System.Reflection.MethodBase> TargetMethods() => BigBrain.BigBrainButtonPatch.TargetMethods();
    [HarmonyPrefix]
    public static bool Prefix(CustomActionButton __instance)
    {
        var local=PlayerControl.LocalPlayer;
        if(!local) return true;
        if(ManipulatorRuntime.Observing(local)) return false;
        return ManipulatorRuntime.VictimSession(local)==null || __instance is not IKillButton || __instance is ManipulatedKillButton;
    }
}
[HarmonyPatch(typeof(KillButton),nameof(KillButton.DoClick))]
public static class ManipulatorVanillaKillGate
{
    [HarmonyPrefix] public static bool Prefix() => ManipulatorRuntime.VictimSession(PlayerControl.LocalPlayer)==null;
}
