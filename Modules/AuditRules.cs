namespace TouMarmo.Modules;

// Pure decisions used by the runtime and executable regression tests.
public static class AuditRules
{
    public static bool ExecutionerCandidate(bool alive, bool connected, bool crew, bool self,
        bool currentTarget, bool reserved, bool spectator) =>
        alive && connected && crew && !self && !currentTarget && !reserved && !spectator;

    public static bool ProtectionCanBlock(bool cancelled, bool kill, bool dirtek, bool bondDeath) =>
        !cancelled && (!kill || !dirtek && !bondDeath);

    public static bool DoctorImmortal(bool revealed, bool enabled, int alive) => revealed && enabled && alive >= 3;

    public static bool OneMoreChanceRole(string role) =>
        role is "SheriffRole" or "OfficerRole" or "JailorRole" or "FurasRole";

    public static bool RollSucceeds(float roll, float percent) => percent > 0 && roll < Math.Clamp(percent, 0, 100);
}
