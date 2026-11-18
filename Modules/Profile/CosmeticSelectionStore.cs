using System.Text.Json;

namespace TouMarmo.Modules.Profile;

/// <summary>Only explicit wardrobe choices replace saved slots; empty means deliberate unequip.</summary>
public sealed class CosmeticSelectionStore
{
    public static readonly string[] Slots = { "hat", "skin", "visor", "pet", "namePlate" };
    public Dictionary<string,string> Values { get; } = new(StringComparer.Ordinal);
    public string FilePath { get; }
    public CosmeticSelectionStore(string filePath)
    {
        FilePath = filePath;
        if (!Read(filePath)) Read(filePath+".bak");
    }
    private bool Read(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            var values=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(path));
            if(values==null) return false;
            foreach(var pair in values)
                if(Slots.Contains(pair.Key) && pair.Value!=null && pair.Value.Length<=1024) Values[pair.Key]=pair.Value;
            return true;
        }
        catch(JsonException){return false;}
    }
    public void Seed(string playerJson)
    {
        using var document=JsonDocument.Parse(playerJson);
        if(!document.RootElement.TryGetProperty("customization",out var customization)) return;
        var changed=false;
        foreach(var slot in Slots)
            if(!Values.ContainsKey(slot) && customization.TryGetProperty(slot,out var item) && item.ValueKind==JsonValueKind.String &&
                item.GetString() is {Length:>0} value && value.Length<=1024)
            {Values[slot]=value;changed=true;}
        if(changed) Save();
    }
    public void Choose(string slot,string? value)
    {
        if(!Slots.Contains(slot)) throw new ArgumentException("Unknown cosmetic slot.",nameof(slot));
        value??=string.Empty;
        if(value.Length>1024) throw new ArgumentException("Cosmetic id too long.",nameof(value));
        Values[slot]=value;
        Save();
    }
    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
        var temporary=FilePath+"."+Guid.NewGuid().ToString("N")+".tmp";
        File.WriteAllText(temporary,JsonSerializer.Serialize(Values,new JsonSerializerOptions{WriteIndented=true}));
        if(File.Exists(FilePath))
        {
            try
            {
                // Do not replace a healthy recovery copy with a corrupt primary file.
                using var parsed=JsonDocument.Parse(File.ReadAllText(FilePath));
                File.Copy(FilePath,FilePath+".bak",true);
            }
            catch(JsonException){ }
        }
        File.Move(temporary,FilePath,true);
    }
}
