namespace TouMarmo.Modules.LobbyMimic;

public sealed record OutfitSnapshot(string Name, int Color, string Hat, string Skin, string Visor, string Pet, string NamePlate);

public sealed class MimicSession
{
    public bool Active { get; private set; }
    public bool Started { get; set; }
    public int Version { get; private set; }
    public int Retired { get; private set; }
    public OutfitSnapshot? Target { get; private set; }
    public Dictionary<uint, OutfitSnapshot> Originals { get; } = new();
    public bool Begin(int version, OutfitSnapshot target)
    {
        if (version <= Retired || version < Version) return false;
        if (!Active) { Originals.Clear(); Started = false; }
        Version = version; Target = target; Active = true; return true;
    }
    public bool Capture(uint id, OutfitSnapshot original) => Active && Originals.TryAdd(id, original);
    public void Stop(int version)
    {
        Retired = Math.Max(Retired, Math.Max(version, Version)); Active = false; Target = null; Started = false;
    }
    public void ResetConnection()
    {
        Active = false; Started = false; Version = 0; Retired = 0;
        Target = null; Originals.Clear();
    }
    public static bool CanCommand(bool host, bool lobby, bool started) => host && lobby && !started;
}
