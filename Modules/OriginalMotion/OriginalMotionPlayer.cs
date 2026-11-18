using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using PowerTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.OriginalMotion;

/// <summary>Presentation only. Never changes transforms or colliders of the role's object.</summary>
[RegisterInIl2Cpp]
public sealed class OriginalMotionPlayer(IntPtr ptr) : MonoBehaviour(ptr)
{
    internal readonly MotionClock Clock = new();
    internal SpriteAnim Owner = null!;
    internal MotionSpec Spec = null!;
    private readonly List<(SpriteRenderer Source, SpriteRenderer Display, Bounds Layout, bool ForcedOff)> renderers = new();
    private Animator animator = null!;
    private bool configured;
    private bool animatorWasEnabled;

    [HideFromIl2Cpp]
    internal void Play(SpriteAnim owner, AnimationClip clip, MotionSpec spec, float speed)
    {
        Owner = owner; Spec = spec;
        Clock.Play(spec.Duration, spec.Loop, speed);
        owner.m_currAnim = clip; owner.m_speed = Clock.Speed;
        animator = owner.GetComponent<Animator>();
        if (!configured && animator) animatorWasEnabled = animator.enabled;
        if (animator) animator.enabled = false;
        if (!configured)
        {
            foreach (var path in spec.SpritePaths)
            {
                var target = string.IsNullOrEmpty(path) ? owner.transform : owner.transform.Find(path);
                var source = target ? target.GetComponent<SpriteRenderer>() : null;
                if (!source) throw new InvalidOperationException("Missing original motion renderer: " + spec.Name + "/" + path);
                var go = new GameObject("MTOU original motion pixels"); go.layer = source!.gameObject.layer;
                go.transform.SetParent(source.transform, false);
                var image = go.AddComponent<SpriteRenderer>();
                renderers.Add((source, image, source.sprite ? source.sprite.bounds : new Bounds(Vector3.zero, Vector3.one), source.forceRenderingOff));
                source.forceRenderingOff = true;
            }
            configured = true;
        }
        Draw();
    }
    private void LateUpdate()
    {
        if (!configured || !Owner) return;
        if (animator) animator.enabled = false;
        Clock.Advance(animator && animator.updateMode == AnimatorUpdateMode.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        Draw();
    }
    [HideFromIl2Cpp]
    internal void Draw()
    {
        if (!configured) return;
        var frame = OriginalMotionCatalog.Frame(Spec, Clock.Frame(Spec.Frames));
        foreach (var item in renderers)
        {
            var source = item.Source; var display = item.Display;
            if (!source || !display) continue;
            display.sprite = frame; display.sharedMaterial = source.sharedMaterial; display.color = source.color;
            display.sortingLayerID = source.sortingLayerID; display.sortingOrder = source.sortingOrder;
            display.maskInteraction = source.maskInteraction;
            display.flipX = source.flipX; display.flipY = source.flipY;
            display.enabled = source.enabled && !item.ForcedOff;
            display.gameObject.layer = source.gameObject.layer;
            var center = item.Layout.center;
            if (source.flipX) center.x = -center.x;
            if (source.flipY) center.y = -center.y;
            display.transform.localPosition = center;
            display.transform.localScale = new Vector3(item.Layout.size.x / Math.Max(.001f, frame.bounds.size.x),
                item.Layout.size.y / Math.Max(.001f, frame.bounds.size.y), 1);
        }
    }
    private void OnEnable()
    {
        if (configured && animator && !animator.keepAnimatorStateOnDisable && !Clock.Stopped) Clock.SetTime(0);
    }
    private void OnDestroy()
    { Release(); }
    [HideFromIl2Cpp]
    internal void Release()
    {
        foreach (var item in renderers)
        {
            if (item.Source) item.Source.forceRenderingOff = item.ForcedOff;
            if (item.Display) Object.Destroy(item.Display.gameObject);
        }
        renderers.Clear(); configured = false; Spec = null!; Clock.Stopped = true;
        if (animator) animator.enabled = animatorWasEnabled;
    }
}
