using System.Numerics;

namespace TouMarmo.Modules;

public static class BodyGroupRules
{
    public static int[] Select(IReadOnlyList<Vector2> positions, Vector2 center, int required, float radius, float spacing)
    {
        if (required <= 0 || required > positions.Count) return Array.Empty<int>();
        var nearby = Enumerable.Range(0, positions.Count)
            .Where(i => Vector2.Distance(center, positions[i]) <= radius)
            .OrderBy(i => Vector2.DistanceSquared(center, positions[i])).ToArray();
        var group = new List<int>();
        int[] result = Array.Empty<int>();
        bool Search(int start)
        {
            if (group.Count == required) { result = group.ToArray(); return true; }
            for (int i = start; i <= nearby.Length - (required - group.Count); i++)
            {
                var candidate = nearby[i];
                if (group.Any(j => Vector2.Distance(positions[j], positions[candidate]) > spacing)) continue;
                group.Add(candidate);
                if (Search(i + 1)) return true;
                group.RemoveAt(group.Count - 1);
            }
            return false;
        }
        Search(0);
        return result;
    }
}
