using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
namespace TouMarmo.Options;
public sealed class ExpandedLobbyOptions : AbstractOptionGroup
{
    public override string GroupName=>"Expanded Lobby";
    public override MenuCategory ParentMenu=>MenuCategory.Game;
    public ModdedNumberOption Capacity {get;}=new("Maximum Players (0 = Game Default)",0f,0f,24f,1f,"Default","#",MiraNumberSuffixes.None,"0",halfIncrements:false);
}
