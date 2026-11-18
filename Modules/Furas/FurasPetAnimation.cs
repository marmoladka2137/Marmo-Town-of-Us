using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Furas;

// Three small, distance-driven walk poses for the original fox sprite.
// The world sprite changes pose; it is not a static image sliding on the map.
public sealed class FurasPetAnimation : IDisposable
{
    private readonly GameObject visual;
    private readonly SpriteRenderer renderer;
    private readonly bool allowed;
    private float walkDistance;
    private float stationaryTime;
    private bool disposed;

    public FurasPetAnimation(Transform root, Sprite sprite, bool visible)
    {
        visual = new GameObject("FurasWalkingFox");
        visual.transform.SetParent(root, false);
        visual.transform.localScale = Vector3.one * 0.52f;
        allowed = visible;
        renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.enabled = visible;
        renderer.sortingOrder = 0;
        visual.layer = root.gameObject.layer;
    }

    public void Tick(float distance, float dt, bool left)
    {
        if (disposed || !visual) return;
        var local = PlayerControl.LocalPlayer;
        bool inSight = allowed && local && local.Data && ShipStatus.Instance;
        if (inSight && !local!.HasDied())
        {
            var origin = local.lightSource ? (Vector2)local.lightSource.transform.position : local.GetTruePosition();
            var position = (Vector2)visual.transform.parent.position;
            inSight = Vector2.Distance(origin,position) <= ShipStatus.Instance.CalculateLightRadius(local.Data)
                && FurasRuntime.ClearPath(origin,position);
        }
        renderer.enabled = inSight;
        if (distance > 0.00001f)
        {
            walkDistance += distance;
            stationaryTime = 0;
        }
        else stationaryTime += dt;
        var moving = walkDistance > 0 && stationaryTime < 0.12f;
        if (!moving) walkDistance = 0;
        var frame = moving ? Mathf.FloorToInt(walkDistance / 0.24f) % 3 : 0;
        var bob = frame == 1 ? 0.045f : 0f;
        var tilt = frame == 1 ? 0f : frame == 2 ? 4f : -4f;
        var stretch = frame == 1 ? 1.06f : 0.97f;
        var root=visual.transform.parent;
        var depth=root.position.y/1000f-root.position.z;
        visual.transform.localPosition = new Vector3(0, bob, depth);
        visual.transform.localRotation = Quaternion.Euler(0, 0, moving ? tilt : 0);
        visual.transform.localScale = new Vector3((left ? -1f : 1f) * 0.52f * stretch,
            0.52f / stretch, 0.52f);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (visual) Object.Destroy(visual);
    }
}
