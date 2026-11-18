// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class DiseasedOptions : AbstractTouModifierOptionGroup<DiseasedModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Diseased", "Diseased");
    public override uint GroupPriority => 21;
    public override Color GroupColor => TownOfUsColors.Diseased;

    [ModdedNumberOption("TouOptionDiseasedKillMultiplier", 1.5f, 5f, 0.5f, MiraNumberSuffixes.Multiplier)]
    public float CooldownMultiplier { get; set; } = 3f;
}