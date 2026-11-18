using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Options;
using TouMarmo.Roles;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Roles;
using UnityEngine;
namespace TouMarmo.Modules.Bandit;
public sealed class BanditState
{
    public int Steals;
    public bool UsedThisRound;
    public float ReadyAt;
    public byte Victim=byte.MaxValue;
    public bool MeetingSeen;
    public byte PendingTarget=byte.MaxValue;
    public bool Pending => PendingTarget!=byte.MaxValue;
    public ushort VictimRole;
    public ModdedRoleTeams VictimTeam;
    public RoleAlignment VictimAlignment;
    public bool VictimCouldKill;
    public RoleSnapshot? SavedRole;
    public RoleBehaviour? OriginalBandit, OriginalVictim;
    public Action<PlayerControl>? RestoreExternal;
    public bool Active => Victim!=byte.MaxValue;
}
// Preserve stateful role fields (charges, marks, targets) when restoring the existing role type.
public sealed class RoleSnapshot
{
    private readonly List<(FieldInfo Field,object? Value)> fields=new();
    private readonly List<(MiraAPI.Hud.CustomActionButton Button,int Uses,float Ready)> buttons=new();
    public RoleSnapshot(RoleBehaviour role)
    {
        if(role.Player.AmOwner) foreach(var button in MiraAPI.Hud.CustomButtonManager.Buttons.Where(b=>b.Enabled(role))) buttons.Add((button,button.UsesLeft,Time.time+button.Timer));
        for(var type=role.GetType();type!=null && type.Assembly==role.GetType().Assembly;type=type.BaseType)
        foreach(var field in type.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
        {
            if(field.IsInitOnly || field.IsLiteral || typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
            try
            {
                var value=field.GetValue(role);
                if(value is Array arr) value=arr.Clone();
                else if(value!=null && value.GetType().Namespace=="System.Collections.Generic")
                {
                    var collection=value.GetType();
                    var ctor=collection.GetConstructors().FirstOrDefault(c=>c.GetParameters().Length==1 && c.GetParameters()[0].ParameterType.IsInstanceOfType(value));
                    if(ctor!=null) value=ctor.Invoke(new[]{value});
                }
                fields.Add((field,value));
            }
            catch(Exception error) { UnityEngine.Debug.LogWarning($"Bandit skipped role field {field.Name}: {error}"); }
        }
    }
    public void Restore(RoleBehaviour role)
    {
        foreach(var (button,uses,ready) in buttons) { button.UsesLeft=uses; button.SetTimer(Math.Max(0,ready-Time.time)); }
        foreach(var (field,value) in fields)
            if(field.DeclaringType!.IsInstanceOfType(role))
                try { field.SetValue(role,value); }
                catch(Exception error) { UnityEngine.Debug.LogWarning($"Bandit could not restore field {field.Name}: {error}"); }
    }
}
public static class BanditRuntime
{
    public static readonly Dictionary<byte,BanditState> States=new();
    internal static bool Restoring;
    public static BanditOptions Options=>OptionGroupSingleton<BanditOptions>.Instance;
    public static BanditState For(PlayerControl p)
    {
        if(!States.TryGetValue(p.PlayerId,out var s)) States[p.PlayerId]=s=new();
        return s;
    }
    public static bool IsBorrower(PlayerControl? p)=>p && States.TryGetValue(p!.PlayerId,out var s) && s.Active;
    public static bool IsBorrowedRole(RoleBehaviour? role)=>role && role!.Player && IsBorrower(role.Player) && role.Player.Data.Role==role;
    public static bool IsVictim(PlayerControl? p)=>p && States.Values.Any(s=>s.Active && s.Victim==p!.PlayerId);
    private static bool IsReserved(PlayerControl p) => IsBorrower(p) || IsVictim(p) ||
        States.Any(pair => pair.Value.Pending && (pair.Key == p.PlayerId || pair.Value.PendingTarget == p.PlayerId));
    public const float StealRange = 1f;
    public static bool CanSteal(PlayerControl p,PlayerControl target)
    {
        if(!MarmoSession.InRound || !MarmoSession.Alive(p) || !MarmoSession.Alive(target) || p==target || p.Data.Role is not BanditRole ||
            IsVictim(p) || IsReserved(target)) return false;
        var s=For(p); var o=Options;
        return NewRoleRules.CanSteal(s.Steals,(int)o.MaxSteals.Value,s.UsedThisRound,s.Active || s.Pending) && Time.time>=s.ReadyAt &&
            (target.IsCrewmate() && o.Crew.Value || target.IsImpostor() && o.Impostor.Value || target.IsNeutral() && o.Neutral.Value) &&
            Vector2.Distance(p.GetTruePosition(),target.GetTruePosition())<=StealRange && FurasRuntime.ClearPath(p.GetTruePosition(),target.GetTruePosition());
    }
    [MethodRpc(1070,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcStealRequest(PlayerControl p,PlayerControl target)
    {
        if(MarmoSession.IsHost && CanSteal(p,target)) RpcQueue(p,target);
    }
    [MethodRpc(1071,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcQueue(PlayerControl p,PlayerControl target)
    {
        var state=For(p);
        if (state.Active || state.Pending) return;
        state.PendingTarget=target.PlayerId;
        state.Steals++;
        state.UsedThisRound=true;
        state.ReadyAt=Time.time+Options.Cooldown.Value;
        state.MeetingSeen=false;
        BigBrainRuntime.Record(p,target,false,(ushort)p.Data.Role.Role);
    }
    [MethodRpc(1073,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcSteal(PlayerControl p,PlayerControl target,ushort roleId)
    {
        if(!MarmoSession.Alive(p) || !MarmoSession.Alive(target) || For(p).Active || IsVictim(target)) return;
        var s=For(p);
        s.PendingTarget=byte.MaxValue;
        s.MeetingSeen=false;
        s.VictimRole=roleId;
        s.VictimTeam=target.Data.Role is ICustomRole custom ? custom.Team : target.IsImpostor() ? ModdedRoleTeams.Impostor : ModdedRoleTeams.Crewmate;
        s.VictimAlignment=target.Data.Role.GetRoleAlignment();
        s.VictimCouldKill=target.Data.Role.CanUseKillButton;
        s.OriginalBandit=p.Data.Role;
        s.OriginalVictim=target.Data.Role;
        s.SavedRole=new RoleSnapshot(target.Data.Role);
        if(target.Data.Role is FurasRole)
        {
            var pet=FurasRuntime.For(target);
            var points=pet.Points.ToArray(); var selected=pet.Selected; var lost=pet.Lost;
            var ready=pet.ReadyAt; var place=pet.PlaceReadyAt; var kill=pet.KillReadyAt;
            s.RestoreExternal=restored=>{var state=FurasRuntime.For(restored);state.Points.AddRange(points);state.Selected=selected;state.Lost=lost;state.ReadyAt=ready;state.PlaceReadyAt=place;state.KillReadyAt=kill;};
        }
        s.Victim=target.PlayerId;
        Restoring=true;
        try
        {
            target.ChangeRole((ushort)RoleTypes.Crewmate);
            p.ChangeRole(roleId);
            Pin(p);
            // The victim retains their real team but the Crewmate role exposes no kill or role abilities.
            target.Data.Role.TeamType=s.VictimTeam==ModdedRoleTeams.Impostor ? RoleTeamTypes.Impostor : RoleTeamTypes.Crewmate;
        }
        finally { Restoring=false; }
        RefreshRole(p);
        RefreshRole(target);
        if(p.AmOwner) MarmoSession.Notify("Role borrowed until the next meeting ends. You still win only with the Crewmates.");
        if(target.AmOwner) MarmoSession.Notify("Bandit stole your role. Your abilities return after the next meeting.");
    }
    private static void RefreshRole(PlayerControl p)
    {
        if (p.AmOwner && HudManager.Instance)
            HudManager.Instance.SetHudActive(p, p.Data.Role, !MeetingHud.Instance && !ExileController.Instance);
    }
    private static void RestoreRole(PlayerControl p, ushort roleId, RoleBehaviour? original)
    {
        if (p.Data.IsDead && p.Data.Role.IsDead)
        {
            p.Data.RoleWhenAlive = new Il2CppSystem.Nullable<RoleTypes>((RoleTypes)roleId);
            if (original) GameHistory.RoleWhenAlive[p.PlayerId] = original!;
            return;
        }
        p.ChangeRole(roleId);
        RefreshRole(p);
    }
    public static void Pin(PlayerControl p)
    {
        if(IsBorrower(p)) p.Data.Role.TeamType=RoleTeamTypes.Crewmate;
    }
    [MethodRpc(1072,LocalHandling=RpcLocalHandling.Before)] public static void RpcRestore(PlayerControl p) => Restore(p.PlayerId);
    public static void Restore(byte id)
    {
        if(!States.TryGetValue(id,out var s) || !s.Active) return;
        var p=MarmoSession.Player(id); var victim=MarmoSession.Player(s.Victim);
        if (s.MeetingSeen) s.UsedThisRound = false;
        s.Victim=byte.MaxValue;
        Restoring=true;
        try
        {
            if(p && p!.Data!=null)
            {
                RestoreRole(p, RoleId.Get<BanditRole>(), s.OriginalBandit);
            }
            if(victim && victim!.Data!=null)
            {
                RestoreRole(victim, s.VictimRole, s.OriginalVictim);
                if (!victim.Data.IsDead)
                {
                    s.SavedRole?.Restore(victim.Data.Role);
                    s.RestoreExternal?.Invoke(victim);
                }
            }
            s.OriginalBandit=null; s.OriginalVictim=null;
            s.SavedRole=null; s.RestoreExternal=null;
        }
        finally { Restoring=false; }
    }
    [RegisterEvent(-1000)] public static void Round(RoundStartEvent e)
    {
        foreach(var state in States.Values)
            if (!state.Active && !state.Pending) state.UsedThisRound=false;
    }
    [MethodRpc(1074,LocalHandling=RpcLocalHandling.Before)]
    public static void RpcCancelPending(PlayerControl p)
    {
        var state=For(p);
        if (!state.Pending) return;
        state.PendingTarget=byte.MaxValue;
        state.MeetingSeen=false;
        state.Steals=Math.Max(0,state.Steals-1);
    }
    public static void Tick()
    {
        foreach(var pair in States.Where(x=>x.Value.Active || x.Value.Pending).ToArray())
        {
            var state=pair.Value;
            var owner=MarmoSession.Player(pair.Key);
            var victim=MarmoSession.Player(state.Pending ? state.PendingTarget : state.Victim);
            if (state.Pending && (!MarmoSession.Alive(owner) || !MarmoSession.Alive(victim)))
            {
                if (MarmoSession.IsHost && owner) RpcCancelPending(owner!);
                else if (!owner) { state.PendingTarget=byte.MaxValue; state.MeetingSeen=false; }
                continue;
            }
            if (!state.Pending && (!owner || !victim || !owner!.Data || !victim!.Data || owner.Data.Disconnected || victim.Data.Disconnected))
            {
                if (MarmoSession.IsHost && owner) RpcRestore(owner!);
                else if (!owner) Restore(pair.Key);
                continue;
            }
            if (MeetingHud.Instance) state.MeetingSeen=true;
            else if (state.MeetingSeen && MarmoSession.InRound && MarmoSession.IsHost)
            {
                // One meeting activates the queued theft. The next returns both roles.
                state.MeetingSeen=false;
                if (state.Pending) RpcSteal(owner!,victim!,(ushort)victim!.Data.Role.Role);
                else { RpcRestore(owner!); state.UsedThisRound=false; }
            }
            else if (state.Active && owner && !owner!.Data.IsDead) Pin(owner);
        }
    }
    public static void RestoreAll(){ foreach(var id in States.Keys.ToArray()) Restore(id); }
    public static void Clear(){ RestoreAll(); States.Clear(); }
}
// Scope every override to the actual borrower instance; native roles keep their original mechanics.
[HarmonyPatch]
public static class BanditTeamPatch
{
    public static IEnumerable<MethodBase> TargetMethods()=>new[]{typeof(TownOfUs.TownOfUsPlugin).Assembly,typeof(BanditRole).Assembly}.SelectMany(a=>a.GetTypes())
        .Where(t=>typeof(RoleBehaviour).IsAssignableFrom(t) && !t.ContainsGenericParameters)
        .Select(t=>t.GetProperty("Team",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)?.GetMethod).Where(m=>m!=null).Cast<MethodBase>();
    [HarmonyPrefix] public static bool Prefix(RoleBehaviour __instance,ref ModdedRoleTeams __result)
    { if(!BanditRuntime.IsBorrowedRole(__instance)) return true; __result=ModdedRoleTeams.Crewmate; return false; }
}
[HarmonyPatch]
public static class BanditWinGate
{
    public static IEnumerable<MethodBase> TargetMethods()=>new[]{typeof(TownOfUs.TownOfUsPlugin).Assembly,typeof(BanditRole).Assembly}.SelectMany(a=>a.GetTypes())
        .Where(t=>typeof(RoleBehaviour).IsAssignableFrom(t) && !t.ContainsGenericParameters)
        .SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        .Where(m=>(m.Name=="WinConditionMet" || m.Name=="get_MetWinCon") && m.ReturnType==typeof(bool));
    [HarmonyPrefix] public static bool Prefix(RoleBehaviour __instance,ref bool __result)
    { if(!BanditRuntime.IsBorrowedRole(__instance)) return true; __result=false; return false; }
}
[HarmonyPatch]
public static class BanditDidWinPatch
{
    public static IEnumerable<MethodBase> TargetMethods()=>new[]{typeof(TownOfUs.TownOfUsPlugin).Assembly,typeof(BanditRole).Assembly,typeof(RoleBehaviour).Assembly}.SelectMany(a=>a.GetTypes())
        .Where(t=>typeof(RoleBehaviour).IsAssignableFrom(t) && !t.ContainsGenericParameters)
        .SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))
        .Where(m=>m.Name=="DidWin" && m.ReturnType==typeof(bool));
    [HarmonyPrefix] public static bool Prefix(RoleBehaviour __instance,object[] __args,ref bool __result)
    {
        if(!BanditRuntime.IsBorrowedRole(__instance)) return true;
        var reason=(GameOverReason)__args[0];
        __result=reason is GameOverReason.CrewmatesByTask or GameOverReason.CrewmatesByVote;
        return false;
    }
}
[HarmonyPatch(typeof(TownOfUs.Utilities.Extensions),nameof(TownOfUs.Utilities.Extensions.ChangeRole))]
public static class BanditRoleChangeGuard
{
    [HarmonyPrefix] public static bool Prefix(PlayerControl player)=>BanditRuntime.Restoring || player.HasDied() || !BanditRuntime.IsVictim(player);
    [HarmonyPostfix] public static void Postfix(PlayerControl player)=>BanditRuntime.Pin(player);
}

[HarmonyPatch]
public static class BanditAlignmentPatch
{
    public static IEnumerable<MethodBase> TargetMethods()=>new[]{typeof(TownOfUs.TownOfUsPlugin).Assembly,typeof(BanditRole).Assembly}.SelectMany(a=>a.GetTypes())
        .Where(t=>typeof(RoleBehaviour).IsAssignableFrom(t) && !t.ContainsGenericParameters)
        .Select(t=>t.GetProperty("RoleAlignment",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)?.GetMethod).Where(m=>m!=null).Cast<MethodBase>();
    [HarmonyPrefix] public static bool Prefix(RoleBehaviour __instance,ref RoleAlignment __result)
    { if(!BanditRuntime.IsBorrowedRole(__instance)) return true; __result=RoleAlignment.CrewmatePower; return false; }
}
[HarmonyPatch]
public static class BanditKillAccessPatch
{
    public static IEnumerable<MethodBase> TargetMethods()=>new[]{typeof(RoleBehaviour).Assembly,typeof(TownOfUs.TownOfUsPlugin).Assembly,typeof(BanditRole).Assembly}.SelectMany(a=>a.GetTypes())
        .Where(t=>typeof(RoleBehaviour).IsAssignableFrom(t) && !t.ContainsGenericParameters)
        .Select(t=>t.GetProperty("CanUseKillButton",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)?.GetMethod).Where(m=>m!=null).Cast<MethodBase>();
    [HarmonyPostfix] public static void Postfix(RoleBehaviour __instance,ref bool __result)
    {
        if(__instance == null || !__instance.Player) return;
        if(BanditRuntime.IsVictim(__instance.Player)) __result=false;
        else if(BanditRuntime.IsBorrowedRole(__instance)) __result=BanditRuntime.For(__instance.Player).VictimCouldKill;
    }
}
[HarmonyPatch(typeof(MiraAPI.Hud.CustomActionButton),nameof(MiraAPI.Hud.CustomActionButton.ClickHandler))]
public static class BanditAbilityLock
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)] public static bool Prefix()=>!BanditRuntime.IsVictim(PlayerControl.LocalPlayer);
}
[HarmonyPatch(typeof(TownOfUs.Modifiers.Game.Assailant.AssassinModifier),nameof(TownOfUs.Modifiers.Game.Assailant.AssassinModifier.OnMeetingStart))]
public static class BanditGuessLock
{
    [HarmonyPrefix] public static bool Prefix(TownOfUs.Modifiers.Game.Assailant.AssassinModifier __instance)=>!BanditRuntime.IsVictim(__instance.Player);
}

