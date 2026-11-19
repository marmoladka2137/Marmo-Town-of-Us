using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Deputy;

[RegisterInIl2Cpp]
public sealed class DeputyShotEffect(IntPtr ptr) : MonoBehaviour(ptr)
{
    public const float Duration = .25f + 1.23f + .16666667f;
    private DeputyShotPresentation.Shot? shot;
    private HorseWrangleOverlay actor = null!;
    private GameObject flame = null!;
    private SpriteRenderer background = null!;
    private Transform closeup = null!;
    private Vector3 closeupStart;
    private Bounds closeupBounds;
    private int screenWidth, screenHeight;
    private readonly List<Material> ownedMaterials = new();
    private float began;
    private bool configured, finished;
    internal byte OwnerId => shot?.Owner.PlayerId ?? byte.MaxValue;

    [HideFromIl2Cpp]
    internal void Configure(DeputyShotPresentation.Shot request)
    {
        shot = request;
        var template = request.Overlay.HorseWrangleAnims.FirstOrDefault();
        if (template == null || !template) throw new InvalidOperationException("HorseWrangle reveal prefab is unavailable.");
        actor = Object.Instantiate(template, transform).Cast<HorseWrangleOverlay>();
        actor.transform.localPosition = template.transform.localPosition;
        actor.transform.localRotation = Quaternion.identity;
        actor.transform.localScale = Vector3.one;
        // The meeting effect uses classic portraits. A player's world body can already
        // have been destroyed when the confirmation reaches another client.
        var victimOutfit = new VisualAppearance(request.Victim.DefaultOutfit, TownOfUsAppearances.Camouflage)
        {
            HatId = "hat_NoHat", SkinId = "skin_None", VisorId = "visor_EmptyVisor",
            PetId = "pet_EmptyPet", PlayerName = string.Empty, NameVisible = false
        };
        var data = new KillOverlayInitData(request.DisplayKiller.DefaultOutfit, PlayerBodyTypes.Classic,
            victimOutfit, PlayerBodyTypes.Classic);
        actor.Initialize(data);
        // Do not use HorseWrangleOverlay.victimSprites: its native getter assumes a different hierarchy.
        // Keep the referenced body objects alive; hiding them does not invalidate the animation's references.
        if (actor.victimParts) actor.victimParts.gameObject.SetActive(false);
        if (actor.killerParts) actor.killerParts.gameObject.SetActive(false);
        closeup = actor.GetComponentsInChildren<Transform>(true).First(t => t.name == "Sheriff Closeup");
        var outfit = closeup.GetComponentsInChildren<SpriteRenderer>(true)
            .First(r => r != actor.impostorForeground && r != actor.impostorHand);
        float oldHeight = outfit.sprite ? outfit.sprite.bounds.size.y : 0f;
        outfit.sprite = TouAssets.DeputyOutfit.LoadAsset();
        if (oldHeight > 0 && outfit.sprite.bounds.size.y > 0)
            outfit.transform.localScale *= oldHeight / outfit.sprite.bounds.size.y;
        bool haveBounds = false;
        foreach (var image in closeup.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!image.sprite) continue;
            var bounds = image.sprite.bounds;
            foreach (float x in new[] { bounds.min.x, bounds.max.x })
            foreach (float y in new[] { bounds.min.y, bounds.max.y })
            {
                var point = closeup.InverseTransformPoint(image.transform.TransformPoint(new Vector3(x, y, 0)));
                if (!haveBounds) { closeupBounds = new Bounds(point, Vector3.zero); haveBounds = true; }
                else closeupBounds.Encapsulate(point);
            }
        }
        FitCloseup();

        background = Object.Instantiate(request.Overlay.background, transform);
        flame = Object.Instantiate(request.Overlay.flameParent, transform);
        var flameImage = flame.GetComponentsInChildren<SpriteRenderer>(true).First(r => r.name == "BackgroundFlame");
        flameImage.sprite = TouAssets.DeputyRevealBg.LoadAsset();
        flameImage.transform.localPosition = new Vector3(0, -2f, 0);
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.gameObject.layer = request.Overlay.gameObject.layer;
            if (!renderer.sharedMaterial) continue;
            var material = new Material(renderer.sharedMaterial);
            renderer.material = material; ownedMaterials.Add(material);
        }
        PlayerMaterial.SetColors(actor.killerParts.ColorId, actor.impostorForeground);
        PlayerMaterial.SetColors(actor.killerParts.ColorId, actor.impostorHand);
        foreach (var collider in GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (var button in GetComponentsInChildren<PassiveButton>(true)) button.enabled = false;
        actor.gameObject.SetActive(false);
        background.enabled = true; flame.SetActive(false);
        began = Time.unscaledTime; configured = true;
        TouAudio.PlaySound(TouAudio.DeputyReveal);
        DeputyShotPresentation.Log.LogDebug($"Begin effect {GetInstanceID()}, source={OwnerId}, target={request.Victim.PlayerId}");
    }

    private void Update()
    {
        if (!configured || finished || shot == null) return;
        try
        {
            if (!shot.Overlay || !shot.Area || shot.Owner.Disconnected || shot.Owner.IsDead ||
                (shot.HadMeeting && (!shot.Meeting || shot.Meeting != MeetingHud.Instance))) { Finish(false); return; }
            float elapsed = Time.unscaledTime - began;
            if (elapsed >= Duration) { Finish(true); return; }
            if (screenWidth != Screen.width || screenHeight != Screen.height) FitCloseup();
            background.enabled = elapsed < 1f / 12f;
            flame.SetActive(elapsed >= 1f / 12f);
            if (elapsed < 1f / 6f)
            {
                flame.transform.localScale = new Vector3(1f, .3f, 1f);
                flame.transform.localEulerAngles = new Vector3(0, 0, 25f);
            }
            else if (elapsed < .25f)
            {
                flame.transform.localScale = new Vector3(1f, .5f, 1f);
                flame.transform.localEulerAngles = new Vector3(0, 0, -15f);
            }
            else
            {
                actor.gameObject.SetActive(elapsed < 1.48f);
                closeup.localPosition = closeupStart + Vector3.right * (.185f * Mathf.Clamp01((elapsed - .25f) / 1.23f));
                flame.transform.localEulerAngles = Vector3.zero;
                flame.transform.localScale = new Vector3(1, 1 - Mathf.Clamp01((elapsed - 1.48f) / .16666667f), 1);
            }
        }
        catch (Exception error)
        {
            DeputyShotPresentation.Log.LogError($"Deputy effect {GetInstanceID()} aborted: {error}");
            Finish(false);
        }
    }

    private void FitCloseup()
    {
        screenWidth = Screen.width; screenHeight = Screen.height;
        var camera = Camera.main;
        float height = camera && camera.orthographic ? camera.orthographicSize * 2 : 6;
        float aspect = camera ? camera.aspect : (float)Math.Max(1, screenWidth) / Math.Max(1, screenHeight);
        var parentScale = closeup.parent.lossyScale;
        float scale = Mathf.Min(height * .68f / Mathf.Max(.01f, closeupBounds.size.y * Mathf.Abs(parentScale.y)),
            height * aspect * .7f / Mathf.Max(.01f, closeupBounds.size.x * Mathf.Abs(parentScale.x)));
        closeup.localScale = new Vector3(scale, scale, 1);
        closeupStart = new Vector3(-closeupBounds.center.x * scale, -closeupBounds.center.y * scale, 0);
        closeup.localPosition = closeupStart;
    }

    [HideFromIl2Cpp]
    internal void Finish(bool completed)
    {
        if (finished) return;
        finished = true;
        DeputyShotPresentation.Log.LogDebug($"Close effect {GetInstanceID()}, completed={completed}");
        gameObject.SetActive(false); // Hide synchronously; Destroy completes at the end of the frame.
        foreach (var material in ownedMaterials) if (material) Object.Destroy(material);
        ownedMaterials.Clear();
        Object.Destroy(gameObject);
        if (shot != null) DeputyShotPresentation.Closed(this, shot, completed);
    }
    private void OnDisable() { if (configured && !finished) Finish(false); }
    private void OnDestroy() { if (!finished) Finish(false); }
}
