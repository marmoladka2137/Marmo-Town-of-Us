// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class SatelliteOptions : AbstractTouModifierOptionGroup<SatelliteModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Satellite", "Satellite");
    public override uint GroupPriority => 34;
    public override Color GroupColor => TownOfUsColors.Satellite;

    [ModdedNumberOption("TouOptionSatelliteButtonCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 15f;

    [ModdedNumberOption("TouOptionSatelliteMaxUses", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxNumCast { get; set; } = 5f;

    [ModdedToggleOption("TouOptionSatelliteOneUsagePerRound")]
    public bool OneUsePerRound { get; set; } = true;

    [ModdedToggleOption("TouOptionSatelliteAllowFirstRound")]
    public bool FirstRoundUse { get; set; } = true;
}
