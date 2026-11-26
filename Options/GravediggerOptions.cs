using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;
namespace TouMarmo.Options;

public sealed class GravediggerOptions : AbstractRoleOptionGroup<GravediggerRole>
{
    public override string GroupName => "Gravedigger";
    public ModdedNumberOption RequiredBodies { get; } = new("Bodies Required to Win", 3f, 1f, 14f, 1f, "#", "#", MiraNumberSuffixes.None, "0", halfIncrements: false);
    public ModdedNumberOption BuryCooldown { get; } = new("Bury Cooldown", 15f, 0f, 120f, 2.5f, "#", "#", MiraNumberSuffixes.Seconds, null, halfIncrements: false);
}
