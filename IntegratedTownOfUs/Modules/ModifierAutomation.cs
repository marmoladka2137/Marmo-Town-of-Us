using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Game.Crewmate;
namespace TownOfUs.Modules;

// Debug tools remain available for explicit Practice selection. They must not
// enter a dummy's random loadout or be inherited as real faction modifiers.
public static class ModifierAutomation
{
    public static bool IsDiagnostic(BaseModifier modifier)=>modifier is TestCleanModifier or TestTimeLordModifier;
    public static TouGameModifier? RememberedFactionModifier(IEnumerable<TouGameModifier> modifiers,bool amneIsAssassin)=>
        modifiers.FirstOrDefault(x=>!IsDiagnostic(x) && x is not AssassinModifier &&
            (x is not DoubleShotModifier || amneIsAssassin));
}
