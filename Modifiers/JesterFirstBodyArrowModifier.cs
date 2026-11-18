using TownOfUs;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class JesterFirstBodyArrowModifier(DeadBody deadBody) : ArrowDeadBodyModifier(deadBody, TownOfUsColors.Jester, 0.5f)
{
    public override string ModifierName => "First Body Arrow";
}
