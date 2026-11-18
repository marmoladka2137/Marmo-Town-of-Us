namespace TouMarmo.Modules.OriginalAnimations;

/// <summary>Original timelines, evaluated in local unscaled seconds; no imported keyframes.</summary>
public static class AnimationTiming
{
    public const float DoctorDuration = 2.1f, DeputyDuration = 1.15f;
    public static float Ease(float t) { t = Math.Clamp(t, 0f, 1f); return t * t * (3f - 2f * t); }
    public static float Ramp(float t, float from, float to) => Ease((t - from) / (to - from));
    public static float Window(float t, float from, float peak, float end) => Ramp(t, from, peak) * (1 - Ramp(t, peak, end));
    public static float DoctorPulse(float t) => Window(t, .08f, .45f, 1.05f) + .6f * Window(t, .95f, 1.3f, 1.95f);
    public static float DeputyOpacity(float t) => Ramp(t, 0, .12f) * (1 - Ramp(t, .88f, DeputyDuration));
    public static float Recoil(float t) => Window(t, .30f, .34f, .53f);
    public static float Flash(float t) => Window(t, .30f, .315f, .40f);
    public static bool Finished(float t, float duration) => !float.IsFinite(t) || t >= duration;
    public static float Fit(float height, float aspect, float parentX, float parentY) =>
        Math.Min(height * .72f / (4.5f * Math.Max(.001f, Math.Abs(parentY))),
                 height * aspect * .82f / (8f * Math.Max(.001f, Math.Abs(parentX))));
}
