// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class RottingOptions : AbstractTouModifierOptionGroup<RottingModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Rotting", "Rotting");
    public override uint GroupPriority => 25;
    public override Color GroupColor => TownOfUsColors.Rotting;

    [ModdedNumberOption("TouOptionRottingTimeBeforeBodyRots", 0f, 25f, 1f, MiraNumberSuffixes.Seconds)]
    public float RotDelay { get; set; } = 5f;
}