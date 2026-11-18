using HarmonyLib;
using MiraAPI.GameOptions;
using TouMarmo.Options;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Roles.Neutral;
namespace TouMarmo.Modules;
[HarmonyPatch(typeof(PestilenceRole), nameof(PestilenceRole.IsGuessable), MethodType.Getter)]
public static class PestilenceGuessability
{
    [HarmonyPostfix] public static void Postfix(ref bool __result) =>
        __result=OptionGroupSingleton<MeetingGuessingOptions>.Instance.PestilenceGuessable.Value;
}
[HarmonyPatch(typeof(AssassinModifier), "IsRoleValid")]
public static class AssassinPestilenceGuess
{
    [HarmonyPrefix] public static bool Prefix(RoleBehaviour role, ref bool __result)
    {
        if (role is not PestilenceRole) return true;
        __result=OptionGroupSingleton<MeetingGuessingOptions>.Instance.PestilenceGuessable.Value;
        return false;
    }
}
[HarmonyPatch(typeof(TownOfUs.Utilities.MiscUtils), nameof(TownOfUs.Utilities.MiscUtils.GetPotentialRoles))]
public static class PestilenceGuessMenu
{
    [HarmonyPostfix] public static void Postfix(ref IEnumerable<RoleBehaviour> __result)
    {
        if (!OptionGroupSingleton<MeetingGuessingOptions>.Instance.PestilenceGuessable.Value) return;
        var roles=__result.ToList();
        if (roles.Any(role=>role is PestilenceRole) || !roles.Any(role=>role is PlaguebearerRole)) return;
        var pest=TownOfUs.Utilities.MiscUtils.AllRegisteredRoles.FirstOrDefault(role=>role is PestilenceRole);
        if (pest) roles.Add(pest!);
        __result=roles;
    }
}
