namespace TouMarmo.Modules.Manipulator;

// Host owns transitions; peers use the same countdown with meeting time paused.
public sealed class ManipulationClock(float seconds)
{
    public float Remaining { get; private set; } = seconds;
    public bool Resolved { get; private set; }
    public float Tail { get; private set; }
    public bool Finished => Resolved && Tail <= 0;
    public bool Advance(float delta, bool meeting)
    {
        if (meeting || delta <= 0 || !float.IsFinite(delta)) return false;
        if (Resolved) { Tail = Math.Max(0, Tail - delta); return false; }
        Remaining = Math.Max(0, Remaining - delta);
        return Remaining <= 0;
    }
    public void Resolve()
    {
        if (Resolved) return;
        Resolved = true; Tail = 3f;
    }
}
