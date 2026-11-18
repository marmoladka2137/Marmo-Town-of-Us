namespace TownOfUs.Modules.Plumber;

/// <summary>Replicated, single-use vent traps. No Unity dependency.</summary>
public sealed class VentSpikeState
{
    public readonly Dictionary<int, byte> Traps = new();
    public readonly Dictionary<byte, int> Hits = new();
    private readonly HashSet<(byte Player, int Vent)> entries = new();
    public int Count(byte owner) => Traps.Values.Count(id => id == owner);
    public bool CanPlace(int vent, byte owner, int max) => !Traps.ContainsKey(vent) && Count(owner) < max;
    public bool Place(int vent, byte owner, int max)
    {
        if (!CanPlace(vent, owner, max)) return false;
        Traps.Add(vent, owner); return true;
    }
    public bool Trigger(int vent, byte player, int threshold, out byte owner, out bool lethal)
    {
        owner = 0; lethal = false;
        if (entries.Contains((player, vent)) || !Traps.Remove(vent, out owner)) return false;
        entries.Add((player, vent));
        var count = Hits.GetValueOrDefault(player) + 1;
        lethal = count >= Math.Max(1, threshold);
        Hits[player] = lethal ? 0 : count;
        return true;
    }
    public void Exit(byte player, int vent) => entries.Remove((player, vent));
    public void ClearEntries() => entries.Clear();
    public void Clear() { Traps.Clear(); Hits.Clear(); entries.Clear(); }
}
