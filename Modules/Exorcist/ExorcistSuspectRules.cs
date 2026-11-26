namespace TouMarmo.Modules.Exorcist;

public static class ExorcistSuspectRules
{
    public static byte[] Decoys(byte killer, byte exorcist, IEnumerable<byte> living) =>
        living.Where(id => id != killer && id != exorcist).Distinct().ToArray();

    public static byte[] Select(byte killer, byte exorcist, IReadOnlyList<byte> living, int choice, bool killerFirst)
    {
        if (killer == exorcist || !living.Contains(killer)) return Array.Empty<byte>();
        var decoys = Decoys(killer, exorcist, living);
        if (choice < 0 || choice >= decoys.Length) return Array.Empty<byte>();
        return killerFirst ? new[] { killer, decoys[choice] } : new[] { decoys[choice], killer };
    }
}
