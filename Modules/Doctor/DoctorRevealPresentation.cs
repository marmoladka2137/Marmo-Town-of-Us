using System.Collections;
using Reactor.Utilities;
using TouMarmo.Modules.OriginalAnimations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Doctor;

internal static class DoctorRevealPresentation
{
    internal const string ModelName = "MarmoDoctorReveal";
    private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("MTOU.DoctorReveal");
    public static void Play(PlayerVoteArea area, bool reveal)
    {
        if (!area || !area.PlayerIcon || !MeetingHud.Instance) return;
        Clear(area);
        if (!reveal) { RestorePlayerIcon(area); return; }
        try
        {
            var model = BuildModel(area, true);
            area.PlayerIcon.gameObject.SetActive(false);
            Coroutines.Start(CoReveal(area, model));
        }
        catch (Exception error)
        {
            RestorePlayerIcon(area);
            Log.LogWarning($"Doctor reveal presentation: {error.Message}");
        }
    }

    internal static GameObject BuildModel(PlayerVoteArea area, bool reveal,
        PlayerMaterial.MaskType mask = PlayerMaterial.MaskType.ComplexUI)
    {
        var model = new GameObject(ModelName); model.layer = area.gameObject.layer;
        model.transform.SetParent(area.transform, false);
        try { model.AddComponent<DoctorRevealView>().Configure(area.PlayerIcon, reveal, mask, area.MaskLayer); }
        catch { Object.Destroy(model); throw; }
        return model;
    }

    private static IEnumerator CoReveal(PlayerVoteArea area, GameObject model)
    {
        var meeting = MeetingHud.Instance;
        float began = Time.unscaledTime;
        try
        {
            while (model && area && meeting && meeting == MeetingHud.Instance && !area.AmDead &&
                   !AnimationTiming.Finished(Time.unscaledTime - began, AnimationTiming.DoctorDuration))
                yield return null;
        }
        finally
        {
            // Never let an old coroutine clear a newer reveal on this vote area.
            bool current = area && area.transform.Find(ModelName)?.gameObject == model;
            if (model) { model.name += " disposed"; model.SetActive(false); Object.Destroy(model); }
            if (current && area && !area.AmDead && area.PlayerIcon) area.PlayerIcon.gameObject.SetActive(true);
        }
    }

    internal static void RestorePlayerIcon(PlayerVoteArea area)
    { Clear(area); if (area && area.PlayerIcon) area.PlayerIcon.gameObject.SetActive(true); }

    public static void Clear(PlayerVoteArea area)
    {
        if (!area) return;
        foreach (var name in new[] { ModelName, "MarmoDoctorRevealLabel" })
        {
            var child = area.transform.Find(name);
            if (child) { child.name = name + " disposed"; child.gameObject.SetActive(false); Object.Destroy(child.gameObject); }
        }
    }
}
