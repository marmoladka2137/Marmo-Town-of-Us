using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities;
using UnityEngine;
using Object=UnityEngine.Object;
namespace TouMarmo.Modules.Papetir;

public static class PapetirRuntime
{
    public sealed record Paper(int Id,byte Owner,byte Appearance,DeadBody Body);
    public static PaperLedger Ledger {get;private set;}=new();
    private static readonly Dictionary<int,Paper> papers=new();
    private static readonly Dictionary<byte,int> created=new();
    private static readonly Dictionary<byte,float> ready=new();
    private static readonly Dictionary<byte,byte> executing=new();
    private static int serial,meeting;
    private static (int id,byte owner,byte reporter)? pendingReport;
    private static float pendingAt;
    public static PapetirOptions Options=>OptionGroupSingleton<PapetirOptions>.Instance;
    public static bool IsPaper(DeadBody? b)=>b && papers.Values.Any(p=>p.Body==b);
    public static float Remaining(PlayerControl p)=>Mathf.Max(0,ready.GetValueOrDefault(p.PlayerId)-Time.time);
    public static int Created(PlayerControl p)=>created.GetValueOrDefault(p.PlayerId);
    public static bool CanCreate(PlayerControl p)=>MarmoSession.InRound && MarmoSession.Alive(p) && p.Data.Role is PapetirRole &&
        p.moveable && !p.inVent && Remaining(p)<=0 && created.GetValueOrDefault(p.PlayerId)<AbilityLimits.Count(Options.BodyLimit.Value);

    [MethodRpc(4500,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcCreate(PlayerControl owner)
    {
        if(!MarmoSession.IsHost || !CanCreate(owner)) return;
        var appearances=PlayerControl.AllPlayerControls.ToArray().Where(p=>MarmoSession.Alive(p)).ToArray();
        if(appearances.Length==0) return;
        var model=appearances[System.Random.Shared.Next(appearances.Length)];
        RpcSpawn(PlayerControl.LocalPlayer,++serial,owner.PlayerId,model.PlayerId,owner.GetTruePosition());
    }
    [MethodRpc(4501,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcSpawn(PlayerControl host,int id,byte owner,byte appearance,Vector2 position)
    {
        if(!host || !host.IsHost() || papers.ContainsKey(id) || !ShipStatus.Instance || MeetingHud.Instance) return;
        var model=MarmoSession.Player(appearance);
        if(!model || !model!.Data || !GameManager.Instance) return;
        var prefabs=GameManager.Instance.deadBodyPrefab;
        if(prefabs==null || prefabs.Length==0 || !prefabs[0]) return;
        var body=Object.Instantiate(prefabs[0]);
        body.ParentId=appearance; body.Reported=false;
        body.name=$"PapetirPaper_{id}";
        body.transform.position=new Vector3(position.x,position.y,position.y/1000f);
        foreach(var renderer in body.bodyRenderers)
        {
            model.SetPlayerMaterialColors(renderer);
            renderer.color=Color.white;
        }
        PaperBodyAppearance.Apply(body);
        papers[id]=new(id,owner,appearance,body);
        created[owner]=created.GetValueOrDefault(owner)+1;
        ready[owner]=Time.time+Options.CreateCooldown.Value;
    }
    private static DeadBody? Closest(PlayerControl p)=>Object.FindObjectsOfType<DeadBody>()
        .Where(b=>b && b.gameObject.activeInHierarchy && !b.Reported &&
            Vector2.Distance(p.GetTruePosition(),b.TruePosition)<=p.MaxReportDistance &&
            FurasRuntime.ClearPath(p.GetTruePosition(),b.TruePosition))
        .OrderBy(b=>Vector2.Distance(p.GetTruePosition(),b.TruePosition)).FirstOrDefault();
    public static bool InterceptReport(PlayerControl reporter,NetworkedPlayerInfo? target)
    {
        if(!reporter.AmOwner || target==null) return true;
        var closest=Closest(reporter);
        var paper=papers.Values.FirstOrDefault(p=>p.Body==closest && p.Appearance==target.PlayerId);
        if(paper==null) return true;
        RpcReport(reporter,paper.Id);return false;
    }
    [MethodRpc(4502,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcReport(PlayerControl reporter,int id)
    {
        if(!MarmoSession.IsHost || !MarmoSession.InRound || !MarmoSession.Alive(reporter) || !reporter.moveable || reporter.inVent ||
            MeetingHud.Instance || (pendingReport!=null && Time.time-pendingAt<5f) || !papers.TryGetValue(id,out var paper) || !paper.Body || paper.Body.Reported || Closest(reporter)!=paper.Body) return;
        var owner=MarmoSession.Player(paper.Owner);var target=MarmoSession.Player(paper.Appearance);
        if(!MarmoSession.Alive(owner) || owner!.Data.Role is not PapetirRole || !target || !target!.Data) return;
        paper.Body.Reported=true;
        pendingReport=(id,paper.Owner,reporter.PlayerId);pendingAt=Time.time;
        // Native body-report meeting presentation, without killing the imitated player.
        reporter.RpcStartMeeting(target.Data);
    }
    [MethodRpc(4503,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcReported(PlayerControl host,int id,byte owner,byte reporter,int reportingMeeting)
    {
        if(!host || !host.IsHost()) return;
        if(Ledger.Report(id,owner,reporter,reportingMeeting,(int)Options.ReportsRequired.Value) &&
            PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.PlayerId==reporter)
            MarmoSession.Notify("You are cursed! You will die at the next meeting unless Papetir dies first.");
    }
    public static void ClearBodies(byte? owner=null)
    {
        foreach(var p in papers.Values.Where(p=>owner==null || p.Owner==owner).ToArray())
        {if(p.Body){p.Body.Reported=true;Object.Destroy(p.Body.gameObject);}papers.Remove(p.Id);}
    }
    public static void Reset(){ClearBodies();Ledger=new();created.Clear();ready.Clear();executing.Clear();pendingReport=null;serial=meeting=0;}
    [RegisterEvent(1000)] public static void Meeting(StartMeetingEvent e)
    {
        meeting++;ClearBodies();
        if(!MarmoSession.IsHost) return;
        if(pendingReport is { } report)
        {
            pendingReport=null;
            if(report.reporter==e.MeetingHud.reporterId && MarmoSession.Alive(MarmoSession.Player(report.owner)))
                RpcReported(PlayerControl.LocalPlayer,report.id,report.owner,report.reporter,meeting);
        }
        foreach(var (ownerId,victimId) in Ledger.Due(meeting))
        {
            var owner=MarmoSession.Player(ownerId);var victim=MarmoSession.Player(victimId);
            Ledger.Resolve(ownerId,victimId);
            if(!MarmoSession.Alive(owner) || owner!.Data.Role is not PapetirRole || !MarmoSession.Alive(victim)) continue;
            executing[victimId]=ownerId;
            owner.RpcSpecialMurder(victim!,MeetingCheck.Ignore,isIndirect:true,ignoreShield:true,resetKillTimer:false,
                createDeadBody:false,teleportMurderer:false,showKillAnim:false,causeOfDeath:"PaperCurse");
        }
    }
    [RegisterEvent] public static void Murder(AfterMurderEvent e)
    {
        if(MarmoSession.IsHost && e.Target && e.Target.HasDied() && executing.Remove(e.Target.PlayerId,out var owner))
            RpcVictim(PlayerControl.LocalPlayer,owner,e.Target.PlayerId);
    }
    [MethodRpc(4504,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcVictim(PlayerControl host,byte owner,byte victim)
    {if(host && host.IsHost()) Ledger.Credit(owner,victim);}
    private static void CancelOwner(byte id)
    {
        var saved=Ledger.Due(int.MaxValue).Any(k=>k.owner==id && PlayerControl.LocalPlayer && k.victim==PlayerControl.LocalPlayer.PlayerId);
        Ledger.Cancel(id);ClearBodies(id);
        if(saved) MarmoSession.Notify("Papetir is gone. Your paper curse is lifted.");
    }
    [RegisterEvent] public static void Death(PlayerDeathEvent e)=>CancelOwner(e.Player.PlayerId);
    [RegisterEvent] public static void Leave(PlayerLeaveEvent e)
    {
        foreach(var id in created.Keys.Where(id=>!MarmoSession.Alive(MarmoSession.Player(id))).ToArray()) CancelOwner(id);
    }
    [RegisterEvent] public static void End(GameEndEvent e)=>ClearBodies();
}

[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.ReportDeadBody))]
public static class PaperReportPatch
{
    [HarmonyPrefix,HarmonyPriority(Priority.First)]
    public static bool Prefix(PlayerControl __instance,NetworkedPlayerInfo target)=>PapetirRuntime.InterceptReport(__instance,target);
}

// Paper is report bait, not a real death to remember, eat or control.
[HarmonyPatch(typeof(MiraAPI.Utilities.Extensions),nameof(MiraAPI.Utilities.Extensions.GetNearestDeadBody))]
public static class PaperAbilityTargetPatch
{
    [HarmonyPostfix] public static void Postfix(PlayerControl playerControl,float radius,ref DeadBody? __result)
    {
        if(!PapetirRuntime.IsPaper(__result)) return;
        __result=Helpers.GetNearestDeadBodies(playerControl.GetTruePosition(),radius,Helpers.CreateFilter(Constants.NotShipMask))
            .Find(body=>body && !body.Reported && !PapetirRuntime.IsPaper(body));
    }
}
