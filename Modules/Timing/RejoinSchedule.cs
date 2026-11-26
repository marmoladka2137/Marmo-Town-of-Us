namespace TouMarmo.Modules.OriginalMotion;

/// <summary>Original MTOU deadline state; no network operations and no copied countdown implementation.</summary>
public sealed class RejoinSchedule
{
    private double deadline;
    public bool Active { get; private set; }
    public void Begin(double now, double seconds)
    {
        if (!double.IsFinite(now) || !double.IsFinite(seconds)) throw new ArgumentOutOfRangeException();
        deadline = now + Math.Max(0, seconds); Active = true;
    }
    public int Seconds(double now) => Active ? (int)Math.Ceiling(Math.Max(0, deadline - now)) : 0;
    public bool Take(double now)
    {
        if (!Active || !double.IsFinite(now) || now < deadline) return false;
        Active = false; return true;
    }
    public void Cancel() => Active = false;
    public static bool Eligible(int selection, bool host) => selection == 0 || selection == (host ? 1 : 2);
}
