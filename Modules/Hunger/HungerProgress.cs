namespace TouMarmo.Modules.Hunger;

// Per modifier instance: meetings accumulate through the match, never across games.
public sealed class HungerProgress
{
    private readonly HashSet<int> meetings = new();
    public int Reports { get; private set; }
    public float Bonus { get; private set; }

    public bool Record(int meetingId, float increment)
    {
        if (!float.IsFinite(increment) || increment < 0 || !float.IsFinite(Bonus + increment) ||
            Reports == int.MaxValue || !meetings.Add(meetingId)) return false;
        Reports++;
        Bonus += increment;
        return true;
    }

    public bool Apply(int reports, float bonus)
    {
        if (reports <= Reports || !float.IsFinite(bonus) || bonus < Bonus) return false;
        Reports = reports;
        Bonus = bonus;
        return true;
    }
}
