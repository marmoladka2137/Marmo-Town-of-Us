namespace TouMarmo.Modules.Jester;

/// <summary>A vote result belongs to a player and role ID, not a replaceable RoleBehaviour.</summary>
public sealed class JesterExileState
{
    public sealed record Result(byte PlayerId, int RoleAtVote, bool EndsGame);
    private Result? pending;
    private (int Meeting, ushort Revision)? ballot;
    private readonly Dictionary<byte, Result> wins = new();
    public void Capture(int meeting, ushort revision, byte? exiled, int roleAtVote, bool wasJester, bool alive, bool tie, bool endsGame)
    {
        if (ballot == (meeting, revision)) return; // A repeated network delivery must not recapture a changed role.
        ballot = (meeting, revision);
        pending = !tie && exiled.HasValue && wasJester && alive ? new Result(exiled.Value, roleAtVote, endsGame) : null;
    }
    public bool Confirm(byte exiled)
    {
        if (pending is not { } result || result.PlayerId != exiled) return false;
        pending = null;
        return wins.TryAdd(exiled, result);
    }
    public bool HasWon(byte id) => wins.ContainsKey(id);
    public bool EndsGame(byte id) => wins.TryGetValue(id, out var result) && result.EndsGame;
    public bool TryGet(byte id, out Result result) => wins.TryGetValue(id, out result!);
    public void BeginMeeting() { pending = null; ballot = null; }
    public void Clear() { BeginMeeting(); wins.Clear(); }
}
