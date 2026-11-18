// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class GiantOptions : AbstractTouModifierOptionGroup<GiantModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Giant", "Giant");
    public override uint GroupPriority => 32;
    public override Color GroupColor => TownOfUsColors.Giant;

    [ModdedNumberOption("TouOptionGiantSpeed", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float GiantSpeed { get; set; } = 0.75f;
}