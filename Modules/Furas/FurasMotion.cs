using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TouMarmo.Modules.Furas;

// Query solid map geometry without changing the global Unity trigger settings.
public sealed class FurasMotion
{
    private readonly Il2CppStructArray<RaycastHit2D> hits = new(32);
    private ContactFilter2D walls;
    private readonly float radius;
    public FurasMotion(int layers, float radius = 0.10f)
    {
        this.radius=Mathf.Max(0.02f,radius);
        walls.SetLayerMask(layers);
        walls.useTriggers = false;
    }

    public bool ClearPath(Vector2 from, Vector2 to)
    {
        var delta = to - from;
        if (delta.sqrMagnitude < 0.00001f) return true;
        var count = Physics2D.CircleCast(from, 0.10f, delta.normalized, walls, hits, delta.magnitude);
        for (var i=0;i<count;i++)
        {
            var collider=hits[i].collider;
            if (collider && !collider.GetComponentInParent<DeadBody>()) return false;
        }
        return true;
    }

    public Vector2 Step(Vector2 position, Vector2 input, float speed, float seconds)
    {
        var remaining = Vector2.ClampMagnitude(input, 1f) * (speed * seconds);
        for (var pass = 0; pass < 2 && remaining.sqrMagnitude > 1e-12f; pass++)
        {
            var distance = remaining.magnitude;
            var direction = remaining / distance;
            var count = Physics2D.CircleCast(position, radius, direction, walls, hits, distance + 0.002f);
            var travel = distance;
            var normal = Vector2.zero;
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.collider && hit.collider.GetComponentInParent<DeadBody>()) continue;
                if (Vector2.Dot(direction, hit.normal) >= -0.0001f) continue;
                var limit = Mathf.Max(0f, hit.distance - 0.002f);
                if (limit >= travel) continue;
                travel = limit;
                normal = hit.normal;
            }
            position += direction * travel;
            if (travel >= distance) break;
            remaining = direction * (distance - travel);
            remaining -= normal * Vector2.Dot(remaining, normal);
        }
        return position;
    }
}
