using TownOfUs.Modifiers;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Modules;
using TouMarmo.Options;
using TownOfUs.Modifiers.Game.Assailant;
using UnityEngine;
namespace TouMarmo.Modifiers;
public sealed class TripleShotModifier : DoubleShotModifier
{
    public int ProtectedMistakes { get; set; }
    public override string IdPrefix => "TouMarmo.Modifier";
    public override string IdPart => "TripleShot";
    public override string ModifierName => "Triple Shot";
    public override string ModifierNameLocale => "TouMarmo.Modifier.TripleShot";
    public override string ModifierMedDescriptionLocale => "TouMarmo.Modifier.TripleShot.MedDescription";
    public override string ModifierWikiDescriptionLocale => "TouMarmo.Modifier.TripleShot.WikiDescription";
    public override LoadableAsset<Sprite>? ModifierIcon => TouMarmo.Assets.MarmoAssets.TripleShot;
    public override string IntroInfo => "You get two extra chances when guessing.";
    public override string GetDescription() => "You have two extra chances when assassinating.";
    public override ModifierUiConfiguration Configuration => new(new Color32(240,120,80,255),
        TmpSpriteUtils.CreateSpriteAsset(TouMarmo.Assets.MarmoAssets.TripleShot.LoadAsset(),"TouMarmo.Modifier.TripleShot",1.45f));
    public override int CustomAmount => (int)OptionGroupSingleton<AssailantModifiersOptions>.Instance.Amount.Value;
    public override bool PreventsOtherModifiers => false;
    public override int CustomChance => (int)OptionGroupSingleton<AssailantModifiersOptions>.Instance.Chance.Value;
    public override int GetAssignmentChance() => CustomChance;
    public override int GetAmountPerGame() => CustomAmount;
    public override void BeforeModifierSpawns() { }
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        var o = OptionGroupSingleton<AssailantModifiersOptions>.Instance;
        return role.Player && role.Player.GetModifierComponent().HasModifier<AssassinModifier>(true) &&
            !role.Player.GetModifierComponent().HasModifier<DoubleShotModifier>(true) &&
            (o.Impostors.Value && role.IsImpostor() || o.Neutrals.Value && role.IsNeutral());
    }
    public override bool IsModifierValidOnPostCheck(RoleBehaviour role) => IsModifierValidOn(role);
    public override void OnActivate()
    {
        base.OnActivate();
        ProtectedMistakes = 0;
    }
}
// Reuse Assassin's original Double Shot branch. Only its Used flag gets a second charge for Triple Shot.
[HarmonyPatch(typeof(DoubleShotModifier), nameof(DoubleShotModifier.Used), MethodType.Setter)]
public static class TripleShotChargePatch
{
    private static bool applying;
    [HarmonyPrefix]
    public static void Prefix(DoubleShotModifier __instance, ref bool value)
    {
        if (applying || !value || __instance is not TripleShotModifier triple) return;
        triple.ProtectedMistakes = Math.Min(2,triple.ProtectedMistakes + 1);
        value = NewRoleRules.TripleShotUsed(triple.ProtectedMistakes);
        if (triple.Player.AmOwner) RpcCharges(triple.Player,triple.ProtectedMistakes);
    }
    [MethodRpc(1091, LocalHandling = RpcLocalHandling.After)]
    public static void RpcCharges(PlayerControl player,int used)
    {
        var triple=player.GetModifier<TripleShotModifier>();
        if (triple==null || used<1 || used>2 || used<triple.ProtectedMistakes) return;
        triple.ProtectedMistakes=used;
        applying=true;
        try { triple.Used=NewRoleRules.TripleShotUsed(used); } finally { applying=false; }
    }
}


// Change only the native Double Shot notice for a Triple Shot holder.
[HarmonyPatch(typeof(MiraLocaleManager),nameof(MiraLocaleManager.Get),new[]{typeof(string),typeof(string)})]
public static class TripleShotNoticePatch
{
    [HarmonyPrefix] public static bool Prefix(string key,ref string __result)
    {
        if(key!="TownOfUsMira.Modifier.DoubleShotPreventedDeath" || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.HasModifier<TripleShotModifier>()) return true;
        __result="Triple Shot prevented your death. "+PlayerControl.LocalPlayer.GetModifier<TripleShotModifier>()!.GetDescription();
        return false;
    }
}
