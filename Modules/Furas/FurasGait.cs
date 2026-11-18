using System;
using System.Numerics;

namespace TouMarmo.Modules.Furas;

// Pure gait math shared with the verification harness. Coordinates are normalized texture UVs.
public sealed class FurasGait
{
    public float Phase { get; private set; }
    public float Blend { get; private set; }
    private float idleTime;
    public void Step(float distance, float dt)
    {
        if (!float.IsFinite(distance) || !float.IsFinite(dt) || dt <= 0) return;
        distance = Math.Clamp(distance, 0f, 0.5f);
        dt = Math.Min(dt, 0.1f);
        var speed = distance / dt;
        var target = Math.Clamp(speed / 0.65f, 0f, 1f);
        Blend += (target - Blend) * (1f - MathF.Exp(-14f * dt));
        Phase = (Phase + distance * (2f * MathF.PI / 0.72f)) % (2f * MathF.PI);
        idleTime += dt;
    }
    private static float Smooth(float a, float b, float value)
    {
        var t = Math.Clamp((value - a) / (b - a), 0f, 1f);
        return t * t * (3 - 2 * t);
    }
    public Vector2 Pose(float x, float y)
    {
        var p = new Vector2(x, y);
        // Two diagonal pairs of paws. Feet translate as units; hips blend back into the torso.
        var legWeight = (1 - Smooth(0.13f, 0.33f, y)) * Smooth(0.10f, 0.18f, x) * (1 - Smooth(0.77f, 0.84f, x));
        var pair = 1 - 2 * Smooth(0.25f, 0.37f, x) + 2 * Smooth(0.55f, 0.69f, x);
        p.X += legWeight * Blend * 0.034f * MathF.Cos(Phase) * pair;
        p.Y += legWeight * Blend * 0.045f * Math.Max(0, MathF.Sin(Phase) * pair);
        // Tail pivots around its attachment to the rump, rather than moving the whole image.
        var tailWeight = (1 - Smooth(0.30f, 0.44f, x)) * Smooth(0.27f, 0.43f, y);
        var angle = (0.035f * MathF.Sin(idleTime * 2f) + Blend * 0.10f * MathF.Sin(Phase + 0.7f)) * tailWeight;
        var tail = p - new Vector2(0.41f, 0.31f);
        p += new Vector2(tail.X * (MathF.Cos(angle) - 1) - tail.Y * MathF.Sin(angle),
            tail.X * MathF.Sin(angle) + tail.Y * (MathF.Cos(angle) - 1));
        var bodyWeight = Smooth(0.11f, 0.35f, y);
        p.Y += bodyWeight * (Blend * 0.010f * MathF.Cos(Phase * 2) + (1 - Blend) * 0.002f * MathF.Sin(idleTime * 2));
        // Small independent head nod keeps the fox's face readable.
        p.Y += Smooth(0.43f, 0.65f, x) * Smooth(0.31f, 0.48f, y) * Blend * 0.006f * MathF.Sin(Phase * 2 + 0.4f);
        return p;
    }
}
