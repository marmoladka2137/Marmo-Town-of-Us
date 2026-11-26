using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace TouMarmo.Modules.Hunger;

public static class HungerEvents
{
    [RegisterEvent]
    public static void Meeting(StartMeetingEvent e)
    {
        // ReportBodyEvent precedes cancellation/validation. Award only when an
        // actual body meeting has started, with the host's accepted report data.
        if (!MarmoSession.IsHost || MeetingRoomManager.Instance == null) return;
        var room = MeetingRoomManager.Instance;
        var reporter = room.reporter;
        if (!room.target || !room.target!.IsDead || !MarmoSession.Alive(reporter) ||
            reporter.PlayerId != e.MeetingHud.reporterId ||
            reporter.GetModifier<HungerModifier>() is not { } hunger) return;
        var increment = OptionGroupSingleton<UniversalModifiersOptions>.Instance.HungerSpeedPerReport.Value;
        if (hunger.Progress.Record(e.MeetingHud.GetInstanceID(), increment))
            RpcProgress(PlayerControl.LocalPlayer, reporter, hunger.Progress.Reports, hunger.Progress.Bonus);
    }

    [MethodRpc(4400, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcProgress(PlayerControl host, PlayerControl reporter, int reports, float bonus)
    {
        if (!host || !host.IsHost() || !reporter || !reporter.Data) return;
        reporter.GetModifier<HungerModifier>()?.Progress.Apply(reports, bonus);
    }
}
