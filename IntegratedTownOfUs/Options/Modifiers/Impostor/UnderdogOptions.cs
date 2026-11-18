// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class UnderdogOptions : AbstractTouModifierOptionGroup<UnderdogModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Underdog", "Underdog");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 43;

    [ModdedNumberOption("TouOptionUnderdogKillCooldownBonus", 2.5f, 10f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldownIncrease { get; set; } = 5f;

    [ModdedToggleOption("TouOptionUnderdogExtraImpsKillCooldown")]
    public bool ExtraImpsKillCooldown { get; set; } = false;
}