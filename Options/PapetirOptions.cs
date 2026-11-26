using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
namespace TouMarmo.Options;
public sealed class PapetirOptions : AbstractRoleOptionGroup<PapetirRole>
{
    public override string GroupName=>"Papetir";
    public ModdedNumberOption CreateCooldown {get;}=new("Create Body Cooldown",20f,0f,120f,2.5f,"#","#",MiraNumberSuffixes.Seconds,null,halfIncrements:false);
    public ModdedNumberOption BodyLimit {get;}=new("Maximum Paper Bodies",6f,0f,30f,1f,"∞","#",MiraNumberSuffixes.None,"0",halfIncrements:false);
    public ModdedNumberOption ReportsRequired {get;}=new("Reports To Curse",2f,2f,3f,1f,"#","#",MiraNumberSuffixes.None,"0",halfIncrements:false);
    public ModdedNumberOption VictimsRequired {get;}=new("Victims Required To Win",2f,1f,15f,1f,"#","#",MiraNumberSuffixes.None,"0",halfIncrements:false);
}
