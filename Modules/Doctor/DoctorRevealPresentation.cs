using System.Collections;
using PowerTools;
using Reactor.Utilities;
using TownOfUs.Modules.RainbowMod;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Doctor;

internal static class DoctorRevealPresentation
{
    internal const string ModelName = "MarmoDoctorReveal";
    internal static readonly Vector3 RevealPosition = new(-0.8f, -0.10f, 0f);
    internal static readonly Vector3 RevealScale = new(0.375f, 0.375f, 1f);
    private static readonly LoadableAudioResourceAsset Sound = new("TouMarmo.Resources.Audio.DoctorRevealChime.wav");

    public static void Play(PlayerVoteArea area, bool reveal)
    {
        if (!area || !area.PlayerIcon || !MeetingHud.Instance) return;
        Clear(area);
        if (!reveal)
        {
            RestorePlayerIcon(area);
            return;
        }
        var model = BuildModel(area, reveal);
        area.PlayerIcon.gameObject.SetActive(false);
        if (reveal) Coroutines.Start(CoReveal(area, model));
    }

    // Uses the actual Mayor body/hand clips and their final pose; no shared MayorPlayer state.
    internal static GameObject BuildModel(PlayerVoteArea area, bool reveal,
        PlayerMaterial.MaskType mask = PlayerMaterial.MaskType.ComplexUI)
    {
        var template = reveal ? TouAssets.MayorRevealPrefab.LoadAsset() : TouAssets.MayorPostRevealPrefab.LoadAsset();
        var model = Object.Instantiate(template, area.transform);
        model.name = ModelName;
        model.transform.localPosition = RevealPosition;
        model.transform.localScale = RevealScale;
        foreach (var child in model.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = area.gameObject.layer;
        var outfit = model.transform.Find("Outfit");
        if (outfit) outfit.gameObject.SetActive(false); // Remove Mayor hat, tie and medal from every frame.
        var source = area.PlayerIcon.cosmetics.currentBodySprite.BodySprite;
        var body = model.GetComponent<SpriteRenderer>();
        foreach (var renderer in model.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (outfit && renderer.transform.IsChildOf(outfit)) continue;
            renderer.material = source.material;
            var rainbow = renderer.gameObject.GetComponent<RainbowBehaviour>() ?? renderer.gameObject.AddComponent<RainbowBehaviour>();
            rainbow.AddRend(renderer, area.PlayerIcon.ColorId);
            renderer.sortingOrder = source.sortingOrder + (renderer == body ? 0 : 3);
        }
        foreach (var animation in model.GetComponentsInChildren<SpriteAnim>(true)) animation.SetSpeed(1.02f);
        model.SetActive(true);
        var player = MarmoSession.Player(area.PlayerId);
        var appearance = player ? player!.Data.DefaultOutfit : null;
        model.AddComponent<DoctorRevealView>().Configure(area.PlayerIcon, body, reveal, mask, area.MaskLayer, appearance);
        return model;
    }

    private static IEnumerator CoReveal(PlayerVoteArea area, GameObject model)
    {
        TouAudio.PlaySound(Sound, .85f);
        yield return null; // SpriteAnim selects its actual default clip in Awake/Start.
        if (!model) yield break;
        var clip = model.GetComponent<SpriteAnim>().GetCurrentAnimation();
        var duration = clip ? clip.length / 1.02f : 2.5f / 1.02f;
        yield return new WaitForSeconds(duration);
        if (!area || !model || area.AmDead || !MeetingHud.Instance) yield break;
        // Restore the player's original outfit with the match-local surgical mask.
        RestorePlayerIcon(area);
    }

    internal static void RestorePlayerIcon(PlayerVoteArea area)
    {
        Clear(area);
        if (area && area.PlayerIcon) area.PlayerIcon.gameObject.SetActive(true);
    }

    public static void Clear(PlayerVoteArea area)
    {
        foreach (var name in new[] { ModelName, "MarmoDoctorRevealLabel" })
        {
            var child = area.transform.Find(name);
            if (child) { child.gameObject.SetActive(false); Object.Destroy(child.gameObject); }
        }
    }
}
