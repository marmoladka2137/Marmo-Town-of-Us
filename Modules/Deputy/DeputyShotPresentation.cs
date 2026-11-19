using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using Reactor.Utilities;
using TownOfUs.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Deputy;

/// <summary>Owns only Deputy's temporary graphics; never changes KillOverlay's shared state.</summary>
public static class DeputyShotPresentation
{
    internal sealed record Shot(KillOverlay Overlay, NetworkedPlayerInfo Owner, NetworkedPlayerInfo DisplayKiller,
        NetworkedPlayerInfo Victim, PlayerVoteArea Area, MeetingHud? Meeting)
    {
        public int OverlayId { get; } = Overlay.GetInstanceID();
        public bool HadMeeting { get; } = Meeting;
    }
    private static readonly Dictionary<int, DeputyShotEffect> Active = new();
    private static readonly Dictionary<int, Queue<Shot>> Queues = new();
    private static readonly HashSet<(int Meeting, byte Source, byte Target)> Seen = new();
    internal static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("MTOU.DeputyShot");

    public static void Play(KillOverlay overlay, NetworkedPlayerInfo owner, NetworkedPlayerInfo displayKiller,
        NetworkedPlayerInfo victim, PlayerVoteArea area)
    {
        if (!overlay || !owner || !displayKiller || !victim || !area) return;
        var meeting = MeetingHud.Instance;
        var key = (meeting ? meeting.GetInstanceID() : area.GetInstanceID(), owner.PlayerId, victim.PlayerId);
        if (!Seen.Add(key)) return;
        var shot = new Shot(overlay, owner, displayKiller, victim, area, meeting);
        int id = overlay.GetInstanceID();
        if (Active.TryGetValue(id, out var current) && current)
        {
            if (!Queues.TryGetValue(id, out var queue)) Queues[id] = queue = new();
            queue.Enqueue(shot);
        }
        else Start(shot);
    }

    private static void Start(Shot shot)
    {
        GameObject? root = null;
        DeputyShotEffect? effect = null;
        try
        {
            root = new GameObject("MarmoDeputyShot");
            root.SetActive(false); // Initialize completely before any temporary sprite can become visible.
            root.layer = shot.Overlay.gameObject.layer;
            root.transform.SetParent(shot.Overlay.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            effect = root.AddComponent<DeputyShotEffect>();
            Active[shot.Overlay.GetInstanceID()] = effect;
            effect.Configure(shot);
            root.SetActive(true);
            Object.Destroy(root, 4f); // Additional local lifetime limit, independent of an RPC response.
        }
        catch (Exception error)
        {
            Log.LogError($"Deputy presentation initialization failed: {error}");
            if (effect) effect!.Finish(false);
            else { if (root) { root!.SetActive(false); Object.Destroy(root); } CompleteVoteArea(shot, false); }
        }
    }

    internal static void Closed(DeputyShotEffect effect, Shot shot, bool completed)
    {
        int id = shot.OverlayId;
        if (Active.TryGetValue(id, out var current) && current == effect) Active.Remove(id);
        if (!shot.Overlay) { Queues.Remove(id); Seen.Clear(); return; }
        try { CompleteVoteArea(shot, completed); }
        catch (Exception error) { Log.LogWarning($"Deputy nameplate completion failed after effect cleanup: {error.Message}"); }
        if (!Queues.TryGetValue(id, out var queue)) return;
        if (!completed || shot.HadMeeting && (!shot.Meeting || shot.Meeting != MeetingHud.Instance)) { Queues.Remove(id); return; }
        if (queue.Count > 0) Start(queue.Dequeue());
        if (queue.Count == 0) Queues.Remove(id);
    }

    private static void CompleteVoteArea(Shot shot, bool completed)
    {
        if (!shot.Area || !shot.Meeting || shot.Meeting != MeetingHud.Instance) return;
        if (completed)
            Coroutines.Start(CustomTouMurderRpcs.CoAnimateDeath(shot.Area, UnityEngine.Random.Range(0, 2), true));
        else CustomTouMurderRpcs.EliminatePlayer(shot.Area);
    }

    public static void CancelAll()
    {
        Queues.Clear();
        foreach (var effect in Active.Values.ToArray()) if (effect) effect.Finish(false);
        Active.Clear(); Seen.Clear();
    }
    [RegisterEvent] public static void Meeting(StartMeetingEvent e) => CancelAll();
    [RegisterEvent] public static void EndMeeting(EndMeetingEvent e) => CancelAll();
    [RegisterEvent] public static void EndGame(GameEndEvent e) => CancelAll();
    [RegisterEvent] public static void Round(RoundStartEvent e) => CancelAll();
    [RegisterEvent] public static void Death(PlayerDeathEvent e)
    {
        foreach (var effect in Active.Values.ToArray())
            if (effect && effect.OwnerId == e.Player.PlayerId) effect.Finish(false);
    }
}
