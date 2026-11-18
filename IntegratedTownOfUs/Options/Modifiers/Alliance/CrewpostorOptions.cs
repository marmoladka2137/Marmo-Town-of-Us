// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class CrewpostorOptions : AbstractTouModifierOptionGroup<CrewpostorModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Crewpostor", "Crewpostor");
    public override uint GroupPriority => 10;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    public ModdedToggleOption CrewpostorReplacesImpostor { get; set; } =
        new("TouOptionCrewpostorReplacesImpostor", true);

    public ModdedToggleOption CanAlwaysSabotage { get; set; } =
        new("TouOptionCrewpostorCanAlwaysSabotage", false);

    public ModdedToggleOption CrewpostorVision { get; set; } =
        new("TouOptionCrewpostorVision", true);

    public ModdedToggleOption ShowsAsImpostor { get; set; } =
        new("TouOptionCrewpostorShowsAsImpostor", false);
}