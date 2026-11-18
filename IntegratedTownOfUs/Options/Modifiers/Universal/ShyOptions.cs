// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class ShyOptions : AbstractTouModifierOptionGroup<ShyModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Shy", "Shy");
    public override uint GroupPriority => 35;
    public override Color GroupColor => TownOfUsColors.Shy;

    [ModdedNumberOption("TouOptionShyTransparencyDelay", 0f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float InvisDelay { get; set; } = 5f;

    [ModdedNumberOption("TouOptionShyTurnTransparentDuration", 0f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float TransformInvisDuration { get; set; } = 5f;

    [ModdedNumberOption("TouOptionShyFinalOpacity", 0f, 80f, 10f, MiraNumberSuffixes.Percent)]
    public float FinalTransparency { get; set; } = 20f;
}