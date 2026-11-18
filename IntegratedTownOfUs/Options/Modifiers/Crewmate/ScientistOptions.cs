// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class ScientistOptions : AbstractTouModifierOptionGroup<ScientistModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Scientist", "Scientist");
    public override uint GroupPriority => 26;
    public override Color GroupColor => TownOfUsColors.Scientist;

    [ModdedToggleOption("TouOptionScientistMoveWithVitals")]
    public bool MoveWithMenu { get; set; } = true;

    [ModdedNumberOption("TouOptionScientistStartingCharge", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float StartingCharge { get; set; } = 20f;

    [ModdedNumberOption("TouOptionScientistRoundCharge", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RoundCharge { get; set; } = 15f;

    [ModdedNumberOption("TouOptionScientistTaskCharge", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TaskCharge { get; set; } = 10f;

    [ModdedNumberOption("TouOptionScientistDisplayCooldown", 0f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float DisplayCooldown { get; set; } = 15f;

    [ModdedNumberOption("TouOptionScientistDisplayDuration", 0f, 30f, 5f, MiraNumberSuffixes.Seconds, zeroInfinity: true)]
    public float DisplayDuration { get; set; } = 15f;
}