using HarmonyLib;
using System.Collections;
using Reactor.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Modules;
using TownOfUs.Networking;
using UnityEngine;
using Object=UnityEngine.Object;
namespace TouMarmo.Modules.Necromancer;
public sealed class NecroState
{
    public int Controls,Reanimations;
    public float ReadyAt,EndsAt,KillAt,NextSync,RepathAt;
    public byte BodyId=byte.MaxValue,KillerId=byte.MaxValue;
    public bool Automatic,Killed,Paused,PreviousMoveable,Attached,AttackPending;
    public byte PendingVictim=byte.MaxValue;
    public float Remaining;
    public DeadBody? Body;
    public NecromancerPuppet? Puppet;
    public readonly List<(SpriteRenderer Renderer,bool Enabled)> BodyRenderers=new();
    public Vector2 Position,Rendered,From,BodyOffset;
    public float PacketAt;
    public FurasMotion? Motion;
    public List<System.Numerics.Vector2> Path=new();
    public bool Active=>BodyId!=byte.MaxValue;
}
public static class NecromancerRuntime
{
    public static readonly Dictionary<byte,NecroState> States=new();
    public static NecromancerOptions Options=>OptionGroupSingleton<NecromancerOptions>.Instance;
    public static NecroState For(PlayerControl p){if(!States.TryGetValue(p.PlayerId,out var s)) States[p.PlayerId]=s=new(); return s;}
    public static DeadBody? FindBody(byte id)=>Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b=>b.ParentId==id && !b.Reported);
    private static DeadBody[] bodies = Array.Empty<DeadBody>();
    private static float refreshAt;
    private static readonly HashSet<byte> pendingBodies=new();
    public static DeadBody? Nearest(PlayerControl p, bool automatic)
    {
        if (Time.unscaledTime >= refreshAt || refreshAt == 0)
        {
            bodies = Object.FindObjectsOfType<DeadBody>().ToArray();
            refreshAt = Time.unscaledTime + 0.12f;
        }
        return bodies.Where(body => body && CanUse(p, body, automatic))
            .OrderBy(body => Vector2.Distance(p.GetTruePosition(), body.TruePosition)).FirstOrDefault();
    }
    public static bool Occupied(byte id)=>States.Values.Any(s=>s.Active && s.BodyId==id);
    public static bool CanUse(PlayerControl p,DeadBody body,bool automatic,float tolerance=0)
    {
        if(!MarmoSession.InRound || !MarmoSession.Alive(p) || p.Data.Role is not NecromancerRole || !body || body.Reported || Occupied(body.ParentId)) return false;
        var dead=MarmoSession.Player(body.ParentId); var s=For(p);
        return dead && dead!.Data.IsDead && !s.Active && Time.time>=s.ReadyAt &&
            (automatic ? s.Reanimations<Options.ReanimateUses.Value : s.Controls<Options.ControlUses.Value) &&
            Vector2.Distance(p.GetTruePosition(),body.TruePosition)<=2.25f+tolerance && FurasRuntime.ClearPath(p.GetTruePosition(),body.TruePosition);
    }
    [MethodRpc(1080,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcRequest(PlayerControl p,byte bodyId,bool automatic)
    {
        if(!MarmoSession.IsHost) return;
        var body=FindBody(bodyId); if(!body || !CanUse(p,body!,automatic,0.25f)) return;
        var killer=GameHistory.KilledPlayers.LastOrDefault(k=>k.VictimId==bodyId);
        RpcBegin(p,bodyId,automatic,killer?.KillerId ?? byte.MaxValue,body!.TruePosition);
    }
    [MethodRpc(1081,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcBegin(PlayerControl p,byte bodyId,bool automatic,byte killerId,Vector2 position)
    {
        if (!FindBody(bodyId))
        {
            if (pendingBodies.Add(p.PlayerId)) Coroutines.Start(WaitForBody(p,bodyId,automatic,killerId,position));
            return;
        }
        BeginLocal(p,bodyId,automatic,killerId,position);
    }
    private static IEnumerator WaitForBody(PlayerControl p,byte id,bool automatic,byte killer,Vector2 position)
    {
        var ship=ShipStatus.Instance;
        var deadline=Time.realtimeSinceStartup+3f;
        while (pendingBodies.Contains(p.PlayerId) && MarmoSession.Alive(p) && ship && ShipStatus.Instance==ship && MarmoSession.InRound &&
            !FindBody(id) && Time.realtimeSinceStartup<deadline)
            yield return null;
        if (ShipStatus.Instance!=ship) yield break;
        var requested=pendingBodies.Remove(p.PlayerId);
        if (requested && MarmoSession.Alive(p) && ShipStatus.Instance==ship && MarmoSession.InRound && FindBody(id))
            BeginLocal(p,id,automatic,killer,position);
    }
    private static void BeginLocal(PlayerControl p,byte bodyId,bool automatic,byte killerId,Vector2 position)
    {
        var body=FindBody(bodyId); var s=For(p);
        if(!body || s.Active || Occupied(bodyId) || !MarmoSession.Alive(p)) return;
        s.Body=body; s.BodyOffset=body!.TruePosition-(Vector2)body.transform.position; s.BodyId=bodyId; s.KillerId=killerId; s.Automatic=automatic; s.Killed=false; s.Paused=false;
        s.Position=s.Rendered=s.From=position; s.PacketAt=Time.time;
        s.EndsAt=Time.time+(automatic?Options.ReanimateDuration.Value:Options.ControlDuration.Value);
        s.AttackPending=false; s.PendingVictim=byte.MaxValue;
        s.NextSync=0;
        s.KillAt=Time.time+Options.KillCooldown.Value; s.Path.Clear(); s.RepathAt=0;
        if(automatic) s.Reanimations++; else s.Controls++;
        var dead=MarmoSession.Player(bodyId); if(dead) BigBrainRuntime.Record(p,dead!,false);
        try
        {
            if (dead)
            {
                s.Puppet=new NecromancerPuppet(dead!.Data.DefaultOutfit,body!.transform,null,p.AmOwner && !automatic);
                s.BodyRenderers.Clear();
                foreach (var renderer in body.bodyRenderers)
                {
                    s.BodyRenderers.Add((renderer,renderer.enabled));
                    renderer.enabled=false;
                }
                if (body.bloodSplatter)
                {
                    s.BodyRenderers.Add((body.bloodSplatter,body.bloodSplatter.enabled));
                    body.bloodSplatter.enabled=false;
                }
            }
        }
        catch (Exception error)
        {
            if(p.AmOwner || MarmoSession.IsHost) RpcStop(p); else Stop(p.PlayerId);
            Error($"Necromancer puppet creation failed: {error}");
            return;
        }
        s.Motion = new FurasMotion(Constants.ShipAndObjectsMask);
        if (p.AmOwner && !automatic)
        {
            try { Attach(p,s); }
            catch (Exception error)
            {
                RpcStop(p);
                Error($"Necromancer camera setup failed: {error}");
            }
        }
    }
    private static void Attach(PlayerControl p,NecroState s)
    {
        s.PreviousMoveable=p.moveable;
        s.Attached=true;
        p.moveable=false; p.MyPhysics.body.velocity=Vector2.zero; p.NetTransform.Halt(); p.NetTransform.SetPaused(true);
        if (s.Body && HudManager.Instance)
        {
            HudManager.Instance.PlayerCam.SetTarget(s.Body);
            if (p.lightSource)
            {
                p.lightSource.transform.SetParent(s.Body!.transform, false);
                p.lightSource.transform.localPosition=new Vector3(s.BodyOffset.x,s.BodyOffset.y,0);
                p.lightSource.Initialize(p.Collider.offset / 2f);
            }
        }
    }
    private static void Detach(PlayerControl p,NecroState s)
    {
        if(!p.AmOwner || !s.Attached) return;
        s.Attached=false;
        p.NetTransform.SetPaused(false);
        p.moveable=s.PreviousMoveable && !MeetingHud.Instance && !ExileController.Instance;
        if(HudManager.Instance) HudManager.Instance.PlayerCam.SetTarget(p);
        if(p.lightSource){p.lightSource.transform.SetParent(p.transform,false); p.lightSource.Initialize(p.Collider.offset/2f);}
    }
    public static void Stop(byte owner)
    {
        if(!States.TryGetValue(owner,out var s) || !s.Active) return;
        var p=MarmoSession.Player(owner); if(p) Detach(p!,s);
        if(s.Body) s.Body!.transform.position=new Vector3(s.Position.x-s.BodyOffset.x,s.Position.y-s.BodyOffset.y,s.Position.y/1000f);
        s.Puppet?.Dispose(); s.Puppet=null;
        foreach(var saved in s.BodyRenderers) if(saved.Renderer) saved.Renderer.enabled=saved.Enabled;
        s.BodyRenderers.Clear();
        s.BodyId=byte.MaxValue; s.Body=null; s.Path.Clear(); s.ReadyAt=Time.time+Options.Cooldown.Value; s.Paused=false;
    }
    [MethodRpc(1083,LocalHandling=RpcLocalHandling.Before)] public static void RpcStop(PlayerControl p) { pendingBodies.Remove(p.PlayerId); Stop(p.PlayerId); }
    [MethodRpc(1082,LocalHandling=RpcLocalHandling.Before)] public static void RpcMove(PlayerControl p,Vector2 position)
    {
        var s=For(p); if(!s.Active || s.Paused || !float.IsFinite(position.x) || !float.IsFinite(position.y) ||
            Vector2.Distance(s.Position,position)>Options.Speed.Value*(Mathf.Max(0.1f,Time.time-s.PacketAt)+0.25f)+0.2f) return;
        if (!s.Automatic && p.AmOwner) return;
        s.From=s.Rendered; s.PacketAt=Time.time; s.Position=position;
    }
    public static PlayerControl? Target(PlayerControl p)
    {
        var s=For(p); if(!s.Active || s.Automatic) return null;
        return PlayerControl.AllPlayerControls.ToArray().Where(t=>t!=p && MarmoSession.Alive(t) && !t.inVent &&
            (Options.CanKillCrew.Value || !t.IsCrewmate()) && Vector2.Distance(s.Position,t.GetTruePosition())<=1.3f && FurasRuntime.ClearPath(s.Position,t.GetTruePosition()))
            .OrderBy(t=>Vector2.Distance(s.Position,t.GetTruePosition())).FirstOrDefault();
    }
    [MethodRpc(1084,LocalHandling=RpcLocalHandling.Before)] public static void RpcAttack(PlayerControl p,PlayerControl target)
    {
        if(!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(p) || p.Data.Role is not NecromancerRole) return;
        var s=For(p); if(!s.Active || s.Automatic || s.Paused || s.Killed || s.AttackPending || Time.time<s.KillAt || !MarmoSession.Alive(target) || target==p ||
            target.inVent || !Options.CanKillCrew.Value && target.IsCrewmate() || Vector2.Distance(s.Position,target.GetTruePosition())>1.45f || !FurasRuntime.ClearPath(s.Position,target.GetTruePosition())) return;
        // Reserve the attempt, not a successful kill. A shield must not spend the only kill.
        s.PendingVictim=target.PlayerId;
        RpcAttackState(PlayerControl.LocalPlayer,p,true,false,Options.KillCooldown.Value);
        p.RpcSpecialMurder(target,MeetingCheck.OutsideMeeting,isIndirect:true,ignoreShield:false,resetKillTimer:false,teleportMurderer:false,showKillAnim:false,causeOfDeath:"Necromancer");
        Coroutines.Start(FinishAttack(p,target,s.BodyId));
    }
    private static IEnumerator FinishAttack(PlayerControl owner,PlayerControl victim,byte bodyId)
    {
        yield return null; yield return null;
        var s=For(owner);
        if(s.Active && s.BodyId==bodyId && s.AttackPending)
            RpcAttackState(PlayerControl.LocalPlayer,owner,false,victim && victim.HasDied(),Mathf.Max(0,s.KillAt-Time.time));
    }
    [RegisterEvent]
    public static void Killed(AfterMurderEvent e)
    {
        if(!MarmoSession.IsHost || !e.Target.HasDied() || e.Source.Data.Role is not NecromancerRole) return;
        var s=For(e.Source);
        if(s.Active && s.AttackPending && s.PendingVictim==e.Target.PlayerId)
            RpcAttackState(PlayerControl.LocalPlayer,e.Source,false,true,Mathf.Max(0,s.KillAt-Time.time));
    }
    [MethodRpc(1085,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcAttackState(PlayerControl host,PlayerControl owner,bool pending,bool killed,float cooldown)
    {
        if(!host || !host.IsHost() || !owner || !float.IsFinite(cooldown)) return;
        var s=For(owner); if(!s.Active) return;
        s.AttackPending=pending; s.Killed|=killed;
        s.KillAt=Time.time+Mathf.Clamp(cooldown,0,Options.KillCooldown.Value);
        if(!pending) s.PendingVictim=byte.MaxValue;
    }
    [RegisterEvent(2000)] public static void Report(ReportBodyEvent e)
    {
        if(e.Target!=null && Occupied(e.Target.PlayerId) && !Options.CanReport.Value) e.Cancel();
    }
    [RegisterEvent(-500)] public static void Meeting(StartMeetingEvent e)
    {
        foreach(var entry in States.Where(x=>x.Value.Active).ToArray())
        {
            if(Options.InterruptOnMeeting.Value){Stop(entry.Key);continue;}
            var p=MarmoSession.Player(entry.Key); if(p) Detach(p!,entry.Value);
            entry.Value.Remaining=Math.Max(0,entry.Value.EndsAt-Time.time); entry.Value.Paused=true;
        }
    }
    [RegisterEvent] public static void Round(RoundStartEvent e)
    {
        foreach(var entry in States.Where(x=>x.Value.Active && x.Value.Paused).ToArray())
        {
            var s=entry.Value; var p=MarmoSession.Player(entry.Key);
            // Meetings normally remove corpses; never recreate or revive a removed body.
            if(!s.Body || !MarmoSession.Alive(p)){Stop(entry.Key); continue;}
            s.Paused=false; s.EndsAt=Time.time+s.Remaining;
            if(p!.AmOwner && !s.Automatic) Attach(p,s);
        }
    }
    public static void Clear(){foreach(var id in States.Keys.ToArray()) Stop(id); States.Clear(); pendingBodies.Clear(); bodies=Array.Empty<DeadBody>(); refreshAt=0;}
    public static void Tick()
    {
        foreach(var entry in States.Where(x=>x.Value.Active).ToArray())
        {
            var s=entry.Value; var p=MarmoSession.Player(entry.Key); var dead=MarmoSession.Player(s.BodyId);
            if(!MarmoSession.Alive(p) || p!.Data.Role is not NecromancerRole || !s.Body || s.Body!.Reported || !dead || !dead!.Data.IsDead || dead.Data.Disconnected)
            {Stop(entry.Key);continue;}
            if(s.Paused || !MarmoSession.InRound) continue;
            if(Time.time>=s.EndsAt){Stop(entry.Key);continue;}
            var controlling=s.Automatic ? MarmoSession.IsHost : p.AmOwner;
            if(controlling)
            {
                Vector2 desired=s.Position;
                if(s.Automatic)
                {
                    var killer=MarmoSession.Player(s.KillerId);
                    if(MarmoSession.Alive(killer))
                    {
                        if(Time.time>=s.RepathAt)
                        {
                            s.RepathAt=Time.time+0.8f;
                            var goal=killer!.GetTruePosition();
                            s.Path=CorpsePathfinder.Find(new(s.Position.x,s.Position.y),new(goal.x,goal.y),
                                (a,b)=>FurasRuntime.ClearPath(new(a.X,a.Y),new(b.X,b.Y)));
                        }
                        if(s.Path.Count>0)
                        {
                            var waypoint=new Vector2(s.Path[0].X,s.Path[0].Y);
                            desired=Vector2.MoveTowards(s.Position,waypoint,Options.Speed.Value*Mathf.Min(Time.deltaTime,0.05f));
                            if(Vector2.Distance(desired,waypoint)<0.08f) s.Path.RemoveAt(0);
                        }
                    }
                    else s.Path.Clear();
                }
                else
                {
                    p.moveable=false; p.MyPhysics.body.velocity=Vector2.zero;
                    desired = s.Motion!.Step(s.Position,FurasRuntime.ReadPetDirection(),Options.Speed.Value,Mathf.Min(Time.deltaTime,0.05f));
                }
                if(!s.Automatic || FurasRuntime.ClearPath(s.Position,desired)) s.Position=desired;
                if(Time.time>=s.NextSync){s.NextSync=Time.time+0.1f; RpcMove(p,s.Position);}
            }
            s.Rendered=controlling?s.Position:Vector2.Lerp(s.From,s.Position,Mathf.Clamp01((Time.time-s.PacketAt)/0.1f));
            // Vanilla/other body updates can re-enable the prone corpse after construction.
            // Enforce the walking representation on every receiver throughout the ability.
            if(s.Puppet!=null) foreach(var saved in s.BodyRenderers) if(saved.Renderer) saved.Renderer.enabled=false;
            s.Puppet?.Tick(s.Rendered);
            s.Body!.transform.position=new Vector3(s.Rendered.x-s.BodyOffset.x,s.Rendered.y-s.BodyOffset.y,s.Rendered.y/1000f);
        }
    }
}

