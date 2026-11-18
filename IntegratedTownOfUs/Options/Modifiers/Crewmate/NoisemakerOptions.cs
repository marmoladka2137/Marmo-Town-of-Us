// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class NoisemakerOptions : AbstractTouModifierOptionGroup<NoisemakerModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Noisemaker", "Noisemaker");
    public override uint GroupPriority => 23;
    public override Color GroupColor => TownOfUsColors.Noisemaker;

    [ModdedToggleOption("TouOptionNoisemakerImpostorsAlerted")]
    public bool ImpostorsAlerted { get; set; } = true;

    [ModdedToggleOption("TouOptionNoisemakerNeutsAlerted")]
    public bool NeutsAlerted { get; set; } = true;

    [ModdedToggleOption("TouOptionNoisemakerCommsAffected")]
    public bool CommsAffected { get; set; } = false;

    [ModdedToggleOption("TouOptionNoisemakerBodyCheck")]
    public bool BodyCheck { get; set; } = true;

    [ModdedNumberOption("TouOptionNoisemakerAlertDuration", 1f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 5f;
}