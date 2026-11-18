// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class BaitOptions : AbstractTouModifierOptionGroup<BaitModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Bait", "Bait");
    public override uint GroupPriority => 20;
    public override Color GroupColor => TownOfUsColors.Bait;

    [ModdedNumberOption("TouOptionBaitMinReportDelay", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MinDelay { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBaitMaxReportDelay", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MaxDelay { get; set; } = 1f;
}