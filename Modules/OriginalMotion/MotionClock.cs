namespace TouMarmo.Modules.OriginalMotion;

/// <summary>Owned playback clock; independent of imported Animator graphs.</summary>
public sealed class MotionClock
{
    public double Elapsed { get; private set; }
    public float Duration { get; private set; }
    public float Speed { get; set; } = 1;
    public bool Loop { get; private set; }
    public bool Paused { get; set; }
    public bool Stopped { get; set; }
    public bool Playing => !Stopped && (Loop || Elapsed < Duration);
    public double Normalized => Duration > 0 ? Elapsed / Duration : 0;
    public void Play(float duration, bool loop, float speed)
    {
        if (!float.IsFinite(duration) || duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
        Duration = duration; Loop = loop; Speed = float.IsFinite(speed) ? Math.Max(0, speed) : 0;
        Elapsed = 0; Paused = false; Stopped = false;
    }
    public void Advance(double delta)
    {
        if (!Playing || Paused || !double.IsFinite(delta) || delta < 0) return;
        SetTime(Elapsed + delta * Math.Max(0, Speed));
    }
    public void SetTime(double time) => Elapsed = double.IsFinite(time) ? Math.Max(0, Loop ? time : Math.Min(time, Duration)) : 0;
    public int Frame(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        double progress = Loop ? Normalized % 1 : Math.Clamp(Normalized, 0, 1);
        return Math.Clamp((int)(progress * (Loop ? count : count - 1)), 0, count - 1);
    }
}
