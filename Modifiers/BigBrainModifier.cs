using MiraAPI.GameOptions;
using TouMarmo.Modules;
using TouMarmo.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Other;
using UnityEngine;
namespace TouMarmo.Modifiers;
public sealed class BigBrainModifier : TouGameModifier
{
    public readonly List<ushort> PendingRoles = new();
    public readonly Dictionary<byte,float> LastDetection = new();
    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "BigBrain";
    public override string ModifierNameLocale => "TouMarmo.Modifier.BigBrain";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.BigBrain.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.BigBrain.WikiDescription";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override LoadableAsset<Sprite>? ModifierIcon => TouMarmo.Assets.MarmoAssets.BigBrain;
    public override ModifierUiConfiguration Configuration => new(new Color32(185,120,220,255),TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.BigBrain.LoadAsset(),"TouMarmo.Modifier.BigBrain",1.45f));
    public override Color FreeplayFileColor => new Color32(185,120,220,255);
    public override bool PreventsOtherModifiers => false;
    public override string GetDescription() => "At meetings, learn which roles targeted you.";
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.BigBrainAmount.Value;
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.BigBrainChance.Value;
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        var o=OptionGroupSingleton<UniversalModifiersOptions>.Instance;
        return role is not SpectatorRole && (role.IsCrewmate() && o.BigBrainCrew.Value || role.IsImpostor() && o.BigBrainImpostor.Value || role.IsNeutral() && o.BigBrainNeutral.Value);
    }
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        var report=PendingRoles.ToArray();
        Reactor.Utilities.Coroutines.Start(DeliverReport(Player, report));
        PendingRoles.Clear(); LastDetection.Clear();
    }
    private static System.Collections.IEnumerator DeliverReport(PlayerControl player, ushort[] report)
    {
        yield return null;
        if (MeetingHud.Instance) BigBrainRuntime.ShowMeetingReport(player, report);
    }
    public override void OnDeath(DeathReason reason) { PendingRoles.Clear(); LastDetection.Clear(); base.OnDeath(reason); }
    public override void OnDeactivate() { PendingRoles.Clear(); LastDetection.Clear(); base.OnDeactivate(); }
}
