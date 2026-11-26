using AmongUs.GameOptions;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using Reactor.Utilities;
using TownOfUs.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Modifiers;
using TouMarmo.Options;
using TownOfUs.Buttons;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Assailant;
using UnityEngine;
namespace TouMarmo.Modules.BigBrain;
public static class BigBrainRuntime
{
    public static void Record(PlayerControl source,PlayerControl target,bool failed,ushort roleId=ushort.MaxValue)
    {
        if (!source || source.Data?.Role == null || !source.AmOwner || source==target || !MarmoSession.Alive(target) || MeetingHud.Instance ||
            !target.HasModifier<BigBrainModifier>() || failed && !OptionGroupSingleton<UniversalModifiersOptions>.Instance.BigBrainFailed.Value) return;
        RpcRecord(source,target,roleId==ushort.MaxValue ? (ushort)source.Data.Role.Role : roleId,failed);
    }
    [MethodRpc(1090,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcRecord(PlayerControl source,PlayerControl target,ushort roleId,bool failed)
    {
        var mod=target.GetModifier<BigBrainModifier>(); var o=OptionGroupSingleton<UniversalModifiersOptions>.Instance;
        if(mod==null || !MarmoSession.Alive(target) || source==target || MeetingHud.Instance || failed && !o.BigBrainFailed.Value || mod.PendingRoles.Count>=AbilityLimits.Count(o.BigBrainDetections.Value)) return;
        if(mod.LastDetection.TryGetValue(source.PlayerId,out var last) && Time.time-last<0.5f) return;
        mod.LastDetection[source.PlayerId]=Time.time;
        if (!mod.PendingRoles.Contains(roleId)) mod.PendingRoles.Add(roleId);
    }
    internal static string BuildReport(IEnumerable<ushort> roleIds)
    {
        var names=FeedbackRules.UniqueRoles(roleIds.Distinct().Select(ResolveRoleName));
        return names.Length==0 ? string.Empty : "Detected roles: "+string.Join(", ",names)+".";
    }
    // Resolve the display name from the live role registry first (works for modded roles),
    // then fall back to the registered role list.
    private static string? ResolveRoleName(ushort id)
    {
        var role=RoleManager.Instance ? RoleManager.Instance.GetRole((RoleTypes)id) : null;
        if(!role) role=MiscUtils.AllRegisteredRoles.FirstOrDefault(r=>(ushort)r.Role==id);
        if(!role) return null;
        var name=role!.GetRoleName();
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }
    public static void ShowMeetingReport(PlayerControl owner,ushort[] roleIds)
    {
        // Mirrors the proven TownOfUs pattern (Lookout/Chef/Doomsayer): add the bubble directly
        // from OnMeetingStart so it lands in the meeting chat on this client only.
        if(!owner || !owner.Data || !owner.AmOwner || !MarmoSession.Alive(owner) || roleIds.Length==0) return;
        if(!HudManager.Instance || !HudManager.Instance.Chat) return;
        var message=BuildReport(roleIds);
        if(message.Length==0) return;
        MiscUtils.AddFakeChat(owner.Data,"<color=#B978DC>Big Brain</color>",message,true,true);
    }
    [RegisterEvent(2000)] public static void Ability(TouAbilityEvent e)
    {
        // Exclude chat/vote/task/report/feedback events that are not uses of an ability on this person.
        var name=e.AbilityType.ToString();
        if((int)e.AbilityType < (int)AbilityType.ShapeshifterShift || name.Contains("Feedback") || name.Contains("Result")) return;
        var failed=name.Contains("Fail") || (e.Result?.Contains("fail",StringComparison.OrdinalIgnoreCase) ?? false) || (e.Result?.Contains("blocked",StringComparison.OrdinalIgnoreCase) ?? false);
        if(e.Target is PlayerControl target) Record(e.Player,target,failed);
        if(e.Target2 is PlayerControl second) Record(e.Player,second,failed);
    }
    [RegisterEvent(3000)] public static void MurderAttempt(BeforeMurderEvent e) => Record(e.Source,e.Target,true);
    [RegisterEvent] public static void MurderSuccess(AfterMurderEvent e) => Record(e.Source,e.Target,false);
    [RegisterEvent(3000)] public static void Cancelled(MiraButtonCancelledEvent e)
    {
        if(e.Button is CustomActionButton<PlayerControl> b && b.Target) Record(PlayerControl.LocalPlayer,b.Target!,true);
    }
}
// Fallback for existing targeted buttons that do not publish TouAbilityEvent.
[HarmonyPatch]
public static class BigBrainButtonPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        var methods=new HashSet<MethodBase>
        {
            AccessTools.Method(typeof(CustomActionButton),nameof(CustomActionButton.ClickHandler))
        };
        foreach(var assembly in new[]{typeof(TownOfUsTargetButton<>).Assembly,typeof(BigBrainButtonPatch).Assembly})
            foreach(var type in assembly.GetTypes().Where(t=>!t.ContainsGenericParameters && typeof(CustomActionButton<PlayerControl>).IsAssignableFrom(t)))
            {
                var method=type.GetMethod(nameof(CustomActionButton.ClickHandler), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if(method!=null) methods.Add(method);
            }
        // Never detour a constructed generic handler. .NET shares these method
        // bodies across target types; patching one instantiation can crash CLR
        // member resolution while Harmony builds the wrapper. The integrated
        // TownOfUsTargetButton handler invokes the same tracking directly.
        return methods.Where(method=>method!=null && method.DeclaringType?.IsGenericType!=true);
    }
    public sealed record Attempt(PlayerControl Source,PlayerControl Target,ushort Role,float Timer,int Uses);
    [HarmonyPrefix] public static void Prefix(CustomActionButton __instance,out Attempt? __state)
    {
        __state=null;
        if(__instance is not CustomActionButton<PlayerControl> button || !PlayerControl.LocalPlayer || !button.Target || !button.CanClick()) return;
        // Murder attempts and confirmations are handled by the native murder pipeline.
        if(button is IKillButton) return;
        __state=new(PlayerControl.LocalPlayer,button.Target!, (ushort)PlayerControl.LocalPlayer.Data.Role.Role,button.Timer,button.UsesLeft);
    }
    [HarmonyPostfix] public static void Postfix(CustomActionButton __instance,Attempt? __state)
    {
        if(__state==null) return;
        var used=__instance.Timer!=__state.Timer || __instance.UsesLeft!=__state.Uses || __instance.EffectActive;
        BigBrainRuntime.Record(__state.Source,__state.Target,!used,__state.Role);
    }
}
[HarmonyPatch(typeof(AssassinModifier),nameof(AssassinModifier.IsModifierGuessable))]
public static class NewModifierGuessPatch
{
    [HarmonyPostfix] public static void Postfix(BaseModifier baseModifier,ref bool __result)
    {
        if(baseModifier is BigBrainModifier or TripleShotModifier or SimpModifier) __result=true;
    }
}
