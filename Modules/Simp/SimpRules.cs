namespace TouMarmo.Modules.Simp;
public static class SimpRules
{
    public static float Multiplier(bool assigned, bool ejected, float penalty) =>
        !assigned ? 1f : ejected ? Math.Clamp(penalty, 1.05f, 3f) : 0.75f;
}
