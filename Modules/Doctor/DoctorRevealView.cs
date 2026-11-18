using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TouMarmo.Modules.OriginalAnimations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Doctor;

/// <summary>Original stationary reveal: two expanding medical pulses and a drawn cross.</summary>
[RegisterInIl2Cpp]
public sealed class DoctorRevealView(IntPtr ptr) : MonoBehaviour(ptr)
{
    private PoolablePlayer source = null!;
    private ProceduralStage? stage;
    private Transform ring = null!, cross = null!;
    private float began;
    private bool animate, configured;

    [HideFromIl2Cpp]
    internal void Configure(PoolablePlayer original, bool reveal, PlayerMaterial.MaskType maskType, int maskLayer)
    {
        source = original; animate = reveal;
        // Clone the unloaded native template, never a live icon with addressable cosmetic handles.
        var model = Object.Instantiate(HudManager.Instance.IntroPrefab.PlayerPrefab, transform);
        model.name = "Doctor native medical uniform";
        model.transform.localPosition = original.transform.localPosition + new Vector3(0, -.10f, 0);
        model.transform.localRotation = original.transform.localRotation;
        model.transform.localScale = original.transform.localScale;
        model.gameObject.SetActive(true);
        model.UpdateFromPlayerOutfit(new NetworkedPlayerInfo.PlayerOutfit
        {
            ColorId = original.ColorId, HatId = DoctorWardrobe.Hat, VisorId = DoctorWardrobe.Visor,
            SkinId = DoctorWardrobe.Skin, PetId = string.Empty, PlayerName = string.Empty, NamePlateId = string.Empty
        }, maskType, false, false, null, true);
        model.SetMaskLayer(maskLayer); model.SetHatAndVisorMaskType(maskType);
        model.ToggleName(false); model.TogglePet(false);
        foreach (var child in model.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = gameObject.layer;
        ProceduralStage.NonInteractive(model.gameObject);
        var body = original.cosmetics.currentBodySprite.BodySprite;
        stage = new ProceduralStage(transform, body.sortingOrder + 8);
        var medical = new Color(.3f, .9f, 1f);
        ring = stage.Ring("Medical pulse", .17f, .015f, medical);
        ring.localPosition = model.transform.localPosition + new Vector3(0, .11f, -.1f);
        cross = stage.Cross("Medical cross", new Vector2(model.transform.localPosition.x + .29f,
            model.transform.localPosition.y + .16f), .16f, medical);
        began = Time.unscaledTime; configured = true;
        if (animate) PresentationSound.Play(true);
    }

    private void LateUpdate()
    {
        if (!configured) return;
        if (!source || !source.transform.parent || !source.transform.parent.gameObject.activeInHierarchy)
        { Object.Destroy(gameObject); return; }
        float t = animate ? Time.unscaledTime - began : AnimationTiming.DoctorDuration;
        if (animate && AnimationTiming.Finished(t, AnimationTiming.DoctorDuration + .1f))
        {
            // Independent local cleanup if the driving coroutine was stopped.
            var area = transform.parent ? transform.parent.GetComponent<PlayerVoteArea>() : null;
            bool current = area && area!.transform.Find(DoctorRevealPresentation.ModelName) == transform;
            gameObject.name += " disposed"; gameObject.SetActive(false); Object.Destroy(gameObject);
            if (current && !area!.AmDead && source) source.gameObject.SetActive(true);
            return;
        }
        ring.localScale = Vector3.one * (1 + .55f * AnimationTiming.Ramp(t, .08f, 1.95f));
        ProceduralStage.Alpha(ring, AnimationTiming.DoctorPulse(t) * .75f);
        cross.localScale = Vector3.one * (.7f + .3f * AnimationTiming.Ramp(t, .2f, .7f));
        ProceduralStage.Alpha(cross, AnimationTiming.Ramp(t, .2f, .6f) * (1 - AnimationTiming.Ramp(t, 1.7f, 2.1f)));
        // The actor never moves or scales: only the medical effects animate.
    }
    private void OnDestroy() => stage?.Dispose();
}

