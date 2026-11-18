using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Assailant;
namespace TouMarmo.Modules.Simp;
[HarmonyPatch(typeof(AssassinModifier), nameof(AssassinModifier.OnActivate))]
public static class ShotAssassin
{
    [HarmonyPostfix] public static void Postfix(AssassinModifier __instance)
    {
        if (!__instance.Player.HasModifier<DoubleShotModifier>()) return;
        var charges = AccessTools.Field(typeof(AssassinModifier), "maxKills");
        if (charges != null && (int)charges.GetValue(__instance)! < 1)
            charges.SetValue(__instance, 1);
        if (__instance.Player.AmOwner && MeetingHud.Instance) __instance.OnMeetingStart();
    }
}

[HarmonyPatch(typeof(AssassinModifier), nameof(AssassinModifier.IsModifierValidOnPostCheck))]
public static class KeepShotAssassin
{
    [HarmonyPostfix] public static void Postfix(RoleBehaviour role, ref bool __result)
    {
        if (role.Player && role.Player.HasModifier<DoubleShotModifier>()) __result = true;
    }
}

// Mira queues additions until FixedUpdate. Include that queue when enforcing uniqueness.
[HarmonyPatch(typeof(ModifierComponent), nameof(ModifierComponent.AddModifier), typeof(BaseModifier))]
public static class SingleAssassin
{
    [HarmonyPrefix] public static bool Prefix(ModifierComponent __instance, BaseModifier modifier, ref BaseModifier? __result)
    {
        if (modifier is not AssassinModifier || !__instance.HasModifier<AssassinModifier>(true)) return true;
        __result = null;
        return false;
    }
}
[HarmonyPatch(typeof(ModifierComponent), "FixedUpdate")]
public static class RepairDuplicateAssassins
{
    [HarmonyPostfix] public static void Postfix(ModifierComponent __instance)
    {
        foreach (var duplicate in __instance.GetModifiers<AssassinModifier>().Skip(1).ToArray())
            __instance.RemoveModifier(duplicate);
    }
}
