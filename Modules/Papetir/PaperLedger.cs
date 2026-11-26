namespace TouMarmo.Modules.Papetir;

public sealed class PaperLedger
{
    private readonly HashSet<int> reports = new();
    private readonly Dictionary<(byte owner, byte victim), int> counts = new();
    private readonly Dictionary<(byte owner, byte victim), int> due = new();
    private readonly Dictionary<byte, HashSet<byte>> victims = new();
    public bool Report(int body, byte owner, byte reporter, int meeting, int threshold)
    {
        if (threshold < 1 || owner==reporter || !reports.Add(body)) return false;
        var key=(owner,reporter);
        counts[key]=counts.GetValueOrDefault(key)+1;
        if (counts[key] < threshold || due.ContainsKey(key)) return false;
        due[key]=meeting+1; return true;
    }
    public (byte owner,byte victim)[] Due(int meeting) => due.Where(p=>p.Value<=meeting).Select(p=>p.Key).ToArray();
    public void Resolve(byte owner, byte victim) { due.Remove((owner,victim)); counts.Remove((owner,victim)); }
    public void Cancel(byte owner)
    {
        foreach(var key in counts.Keys.Where(k=>k.owner==owner).ToArray()) counts.Remove(key);
        foreach(var key in due.Keys.Where(k=>k.owner==owner).ToArray()) due.Remove(key);
    }
    public bool Credit(byte owner,byte victim)
    {
        if(owner==victim) return false;
        if(!victims.TryGetValue(owner,out var set)) victims[owner]=set=new();
        return set.Add(victim);
    }
    public int Victims(byte owner) => victims.TryGetValue(owner,out var set) ? set.Count : 0;
}
