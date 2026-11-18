// MTOU publication snapshot prepared 2026-11-17.
// Modified relative to TOU Mira b56f24a0; original credits retained. See SOURCE_PROVENANCE.md.
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class AllianceModifierOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.AllianceModifiers");
    public override Func<bool> GroupVisible => () => RoleOptions.GetIsClassicRoleAssignment();
    public override Color GroupColor => Color.white;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 0;

    public AmountChanceOption CrewpostorChance { get; } = new("TouOptionCrewpostorChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Impostor, asset: TouModifierIcons.Crewpostor,
        assetName: "TouMira.Modifier.Alliance.Crewpostor", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance;
            RunNotif(opt, x > 0f ? "1" : "0", "TownOfUsMira.Modifier.Crewpostor");
        }
    };

    public AmountChanceOption EgotistChance { get; } = new("TouOptionEgotistChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Egotist, asset: TouModifierIcons.Egotist,
        assetName: "TouMira.Modifier.Alliance.Egotist", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
            RunNotif(opt, x > 0f ? "1" : "0", "TownOfUsMira.Modifier.Egotist");
        }
    };

    public AmountChanceOption LoversChance { get; } = new("TouOptionLoversChance", 0, 0, 100f, 10f, "#", "#",
        MiraNumberSuffixes.Percent, color: TownOfUsColors.Lover, asset: TouModifierIcons.Lover,
        assetName: "TouMira.Modifier.Alliance.Lover", assetScale: 1.45f)
    {
        ChangedEvent = x =>
        {
            var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance;
            RunNotif(opt, x > 0f ? "2" : "0", "TownOfUsMira.Modifier.Lover");
        }
    };
    private static void RunNotif(AmountChanceOption opt, string count, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            MiraLocaleManager.Get(title),
            count,
            opt.Data.GetValueString(opt.Value));
    }
};
