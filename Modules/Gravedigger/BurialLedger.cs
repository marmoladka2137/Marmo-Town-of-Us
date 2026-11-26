namespace TouMarmo.Modules.Gravedigger;

// Per-game immutable corpse-role identities; does not depend on living role instances.
public sealed class BurialLedger
{
    private readonly Dictionary<byte, ushort> buried = new();
    public IReadOnlyDictionary<byte, ushort> Buried => buried;
    public int Count => buried.Count;
    public bool Inherited { get; private set; }
    public ushort ChosenRole { get; private set; }
    public bool Bury(byte body, ushort role) => !Inherited && buried.TryAdd(body, role);
    public bool CanInherit(ushort role, ushort gravedigger) => !Inherited && role != gravedigger && buried.Values.Contains(role);
    public bool Inherit(ushort role, ushort gravedigger)
    {
        if (!CanInherit(role, gravedigger)) return false;
        Inherited = true; ChosenRole = role; return true;
    }
    public bool Won(bool alive, int required) => alive && !Inherited && required > 0 && Count >= required;
}
