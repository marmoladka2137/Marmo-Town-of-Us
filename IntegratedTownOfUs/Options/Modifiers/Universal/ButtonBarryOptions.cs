// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class ButtonBarryOptions : AbstractTouModifierOptionGroup<ButtonBarryModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.ButtonBarry", "Button Barry");
    public override uint GroupPriority => 30;
    public override Color GroupColor => TownOfUsColors.ButtonBarry;

    [ModdedNumberOption("TouOptionButtonBarryButtonCooldown", 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionButtonBarryMaxUses", 0f, 3f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxNumButtons { get; set; } = 1f;

    [ModdedToggleOption("TouOptionButtonBarryIgnoreSabotage")]
    public bool IgnoreSabo { get; set; } = true;

    [ModdedToggleOption("TouOptionButtonBarryAllowFirstRound")]
    public bool FirstRoundUse { get; set; } = false;
}
