// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PlumberOptions : AbstractRoleOptionGroup<PlumberRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Plumber", "Plumber");

    [ModdedNumberOption("TouOptionPlumberFlushCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float FlushCooldown { get; set; } = 25f;

    [ModdedNumberOption("Plumber Hits Required to Kill", 1f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float HitsRequiredToKill { get; set; } = 3f;

    [ModdedNumberOption("Plumber Max Trapped Vents", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTrappedVents { get; set; } = 3f;

    [ModdedNumberOption("Plumber Trap Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float TrapCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionPlumberFlushDuration", 1f, 20f, 1f, MiraNumberSuffixes.Seconds, "0.0")]
    public float FlushDuration { get; set; } = 3f;

    [ModdedToggleOption("TouOptionPlumberGetMoreFromTasks")]
    public bool TaskUses { get; set; } = true;
}
