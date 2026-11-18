// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class MiniOptions : AbstractTouModifierOptionGroup<MiniModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Mini", "Mini");
    public override uint GroupPriority => 33;
    public override Color GroupColor => TownOfUsColors.Mini;

    [ModdedNumberOption("TouOptionMiniSpeed", 1.05f, 2.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float MiniSpeed { get; set; } = 1.35f;
}