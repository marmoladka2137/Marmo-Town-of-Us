// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules.Plumber;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class PlumberEvents
{
    [RegisterEvent] public static void Round(RoundStartEvent e)
    {
        if (e.TriggeredByIntro) PlumberRole.ClearAll();
    }
    [RegisterEvent] public static void End(GameEndEvent e) => PlumberRole.ClearAll();
    [RegisterEvent] public static void Meeting(StartMeetingEvent e) => PlumberNailRuntime.ClearEntries();
    [RegisterEvent] public static void CompleteTask(CompleteTaskEvent e)
    {
        if (!e.Player.AmOwner || e.Player.Data.Role is not PlumberRole || !OptionGroupSingleton<PlumberOptions>.Instance.TaskUses) return;
        var button = CustomButtonSingleton<PlumberNailButton>.Instance;
        ++button.ExtraUses;
        button.SetUses(button.UsesLeft + 1);
    }
    [RegisterEvent] public static void CanUse(PlayerCanUseEvent e)
    {
        if (e.IsVent && e.Usable.TryCast<Vent>() is { } vent && PlumberRole.VentFlushSet.Contains(vent.Id)) e.Cancel();
    }
    [RegisterEvent] public static void Ejected(EjectionEvent e) => PlumberRole.VentFlushSet.Clear();
}
