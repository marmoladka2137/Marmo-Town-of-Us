using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs;
using TownOfUs.Networking;
using TouMarmo.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TouMarmo.Modifiers;

public sealed class OneMoreChanceModifier : TouGameModifier
{
    public bool Used { get; set; }
    private static readonly HashSet<(byte Owner, byte Victim)> Forgiven = new();
    public override bool HideOnUi => Used;
    public static bool WasForgiven(PlayerControl owner, PlayerControl victim) => Forgiven.Contains((owner.PlayerId, victim.PlayerId));
    public static void ClearHistory() => Forgiven.Clear();

    // Must be called by the action owner or host only, never by every result recipient.
    public static bool Consume(PlayerControl owner, PlayerControl victim)
    {
        var mod = owner.GetModifier<OneMoreChanceModifier>();
        if (mod == null || !mod.TryConsume()) return false;
        Forgiven.Add((owner.PlayerId, victim.PlayerId));
        RpcConsume(owner, victim.PlayerId);
        return true;
    }

    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "OneMoreChance";
    public override string ModifierName => "One More Chance";
    public override string ModifierNameLocale => "TouMarmo.Modifier.OneMoreChance";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.OneMoreChance.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.OneMoreChance.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => TouMarmo.Assets.MarmoAssets.OneMoreChance;
    public override ModifierUiConfiguration Configuration => new(
        new Color32(100, 200, 100, 255),
        TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.OneMoreChance.LoadAsset(), "TouMarmo.Modifier.OneMoreChance", 1.45f));
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(100, 200, 100, 255);
    public override bool PreventsOtherModifiers => false;
    public override bool AppearsInSummary => true;
    public override bool AppearsInIntro => true;
    public override bool HideFromGuessing => false;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        // Only valid on Sheriff, Officer, Jailor, Furas
        if (role is null || role.Player is null) return false;
        var roleName = role.GetType().Name;
        return AuditRules.OneMoreChanceRole(roleName)
            && base.IsModifierValidOn(role);
    }

    public override bool IsModifierValidOnPostCheck(RoleBehaviour role) => IsModifierValidOn(role);

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.OneMoreChanceChance.Value;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifiersOptions>.Instance.OneMoreChanceCount.Value;

    public override string GetDescription()
    {
        return Used
            ? "Your extra chance has been used."
            : "You have one extra chance after a mistake.";
    }

    public override void OnActivate()
    {
        Used = false;
    }

    /// <summary>
    /// Tries to consume the one more chance. Returns true if consumed (protection applied).
    /// </summary>
    public bool TryConsume()
    {
        if (Used) return false;
        Used = true;
        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.OneMoreChanceConsume, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcConsume(PlayerControl player, byte victimId)
    {
        if (!player) return;
        Forgiven.Add((player.PlayerId, victimId));
        var mod = player.GetModifier<OneMoreChanceModifier>();
        if (mod == null) return;
        mod.Used = true;
        // Remove the modifier from the player after consumption
        player.RemoveModifier(mod);
        if (player.AmOwner)
        {
            // Single, central "chance used" notification for every role (Sheriff/Officer/Jailor/Furas).
            // The individual roles must NOT show their own copy of this or the player sees it twice.
            var notif = Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TouMarmo.Modifier.OneMoreChance.Used", "You lost your extra chance."),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouMarmo.Assets.MarmoAssets.OneMoreChance.LoadAsset());
            notif.AdjustNotification();
        }
    }
}