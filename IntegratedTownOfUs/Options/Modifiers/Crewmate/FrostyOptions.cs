// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class FrostyOptions : AbstractTouModifierOptionGroup<FrostyModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Frosty", "Frosty");
    public override uint GroupPriority => 22;
    public override Color GroupColor => TownOfUsColors.Frosty;

    [ModdedNumberOption("TouOptionFrostyChillDuration", 0f, 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ChillDuration { get; set; } = 10f;

    [ModdedNumberOption("TouOptionFrostyChillStartSpeed", 0.25f, 0.95f, 0.05f,
        MiraNumberSuffixes.Multiplier, "0.00")]
    public float ChillStartSpeed { get; set; } = 0.75f;
}