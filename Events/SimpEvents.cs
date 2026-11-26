using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Buttons;
using TouMarmo.Modifiers;
using TouMarmo.Modules;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Buttons;
using TownOfUs.Roles.Other;
using UnityEngine;
namespace TouMarmo.Events;
public static class SimpEvents
{
    public static float Multiplier(PlayerControl? player)
    {
        if (!player || player!.Data?.Role == null || !player.Data.Role.IsImpostor()) return 1f;
        var mod = player.GetModifier<SimpModifier>();
        return mod == null ? 1f : SimpRules.Multiplier(mod.TargetId != byte.MaxValue, mod.TargetEjected,
            OptionGroupSingleton<ImpostorModifiersOptions>.Instance.SimpPenalty.Value);
    }
    [RegisterEvent(100)]
    public static void RoundStart(RoundStartEvent ev)
    {
        AssignMissingTargets();
        var local = PlayerControl.LocalPlayer;
        if (local && !local.HasDied() && local.GetModifier<SimpModifier>() is { } simp && local.Data.Role.IsImpostor())
            RefreshCooldown(local);
    }
    public static void AssignMissingTargets()
    {
        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            foreach (var owner in PlayerControl.AllPlayerControls.ToArray())
            {
                var mod = owner.GetModifier<SimpModifier>();
                if (owner.Data?.Role == null || mod == null || mod.TargetId != byte.MaxValue || owner.HasDied() || !owner.Data.Role.IsImpostor()) continue;
                var candidates = PlayerControl.AllPlayerControls.ToArray().Where(p => p != owner && p.Data != null &&
                    !p.HasDied() && !p.Data.Disconnected && p.Data.Role is not SpectatorRole).ToArray();
                if (candidates.Length > 0) RpcAssign(owner, candidates[System.Random.Shared.Next(candidates.Length)]);
            }
        }
    }
    [MethodRpc(1040, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAssign(PlayerControl owner, PlayerControl target)
    {
        if (!owner || !target || target.Data == null || target.Data.Disconnected) return;
        var mod = owner.GetModifier<SimpModifier>();
        if (mod == null || mod.TargetId != byte.MaxValue || owner == target || target.HasDied()) return;
        mod.TargetId = target.PlayerId;
        mod.TargetEjected = false;
        if (owner.AmOwner) { RefreshCooldown(owner); Notify(mod.GetDescription()); }
    }
    [RegisterEvent(100)]
    public static void Ejection(EjectionEvent ev)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var target = ev.ExileController?.initData?.networkedPlayer?.Object;
        if (!target || SoulguardRole.FindLivingGuard(target!) != null) return;
        foreach (var owner in PlayerControl.AllPlayerControls.ToArray())
            if (owner.GetModifier<SimpModifier>() is { TargetEjected: false } mod && mod.TargetId == target!.PlayerId)
                RpcFailed(owner);
    }
    [MethodRpc(1041, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcFailed(PlayerControl owner)
    {
        var mod = owner.GetModifier<SimpModifier>();
        if (mod == null || mod.TargetEjected || mod.TargetId == byte.MaxValue) return;
        mod.TargetEjected = true;
        if (owner.AmOwner) Notify("Your Simp target was voted out. Your kill cooldown is now longer.");
    }
    [RegisterEvent(1)]
    public static void Killed(AfterMurderEvent ev)
    {
        if (!ev.Source.AmOwner || MeetingHud.Instance || !ev.Source.HasModifier<SimpModifier>() ||
            !ev.Source.Data.Role.IsImpostor() || !ev.Source.Data.Role.CanUseKillButton) return;
        ev.Source.SetKillTimer(ev.Source.GetKillCooldown());
    }
    private static void RefreshCooldown(PlayerControl player)
    {
        if (player.Data.Role.CanUseKillButton) player.SetKillTimer(player.GetKillCooldown());
        foreach (var button in CustomButtonManager.Buttons)
            if (button is IKillButton && button.Enabled(player.Data.Role)) button.SetTimer(button.Cooldown);
    }
    private static void Notify(string message) => Helpers.CreateAndShowNotification(message, Color.white,
        new Vector3(0, 1, -20), spr: TouMarmo.Assets.MarmoAssets.Simp.LoadAsset());
}

// Apply once at the outermost cooldown getter, even when a role-specific getter calls GetKillCooldown.
[HarmonyPatch]
public static class SimpCooldownPatch
{
    [ThreadStatic] private static int depth;
    public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TownOfUs.Utilities.Extensions), "GetKillCooldown");
        yield return AccessTools.Method(typeof(TownOfUs.Utilities.Extensions), "GetReducedKillCooldown");
        foreach (var type in typeof(TownOfUs.TownOfUsPlugin).Assembly.GetTypes().Where(t => !t.IsAbstract &&
            typeof(CustomActionButton).IsAssignableFrom(t) && (typeof(IKillButton).IsAssignableFrom(t) || typeof(IDiseaseableButton).IsAssignableFrom(t))))
        {
            var getter = type.GetProperty("Cooldown", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)?.GetMethod;
            if (getter != null) yield return getter;
        }
        yield return AccessTools.PropertyGetter(typeof(TouMarmo.Buttons.HuggerMapButton), "Cooldown");
    }
    [HarmonyPrefix]
    public static void Prefix(object? __instance, object[] __args, out float __state)
    {
        depth++;
        var player = __args.Length > 0 ? __args[0] as PlayerControl : PlayerControl.LocalPlayer;
        __state = player && (__instance is not CustomActionButton b || player!.Data?.Role != null && b.Enabled(player.Data.Role))
            ? SimpEvents.Multiplier(player) : 1f;
    }
    [HarmonyPostfix]
    public static void Postfix(ref float __result, float __state)
    {
        if (depth == 1) __result *= __state;
    }
    [HarmonyFinalizer]
    public static void Finalizer() => depth = Math.Max(0, depth - 1);
}
