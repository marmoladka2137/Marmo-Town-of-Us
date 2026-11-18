using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TouMarmo.Modules;

public enum ProtectionKind : byte
{
    Medic, Warden, Cleric, Doctor, Dirtek, GuardianAngel, Survivor, FirstDead,
    Herbalist, Mirrorcaster, Mercenary, Monarch, Invulnerable, Veteran, VanillaGuardian
}

/// <summary>Shared rejection and presentation policy; shield owners still control their own charges.</summary>
public static class ProtectionRuntime
{
    private static readonly Dictionary<(byte, byte, ProtectionKind), float> LastFlash = new();
    public static readonly HashSet<byte> BondDeaths = new();
    private static SpriteRenderer? overlay;
    private static int flashGeneration;

    public static bool Bypass(PlayerControl target) => target &&
        (target.Data?.Role is SoulguardRole || BondDeaths.Contains(target.PlayerId));

    public static bool MayDefend(MiraCancelableEvent e, PlayerControl target)
    {
        if (e.IsCancelled) return false;
        var kill = e is BeforeMurderEvent || e is MiraButtonClickEvent click && click.Button is IKillButton;
        return AuditRules.ProtectionCanBlock(e.IsCancelled, kill, target.Data?.Role is SoulguardRole, BondDeaths.Contains(target.PlayerId));
    }

    public static void Prepare(BeforeMurderEvent e, bool didSucceed, MeetingCheck meeting)
    {
        bool active = MeetingHud.Instance || ExileController.Instance;
        if (!didSucceed || !MarmoSession.Alive(e.Target) ||
            meeting == MeetingCheck.ForMeeting && !active || meeting == MeetingCheck.OutsideMeeting && active)
            e.Cancel();
    }

    public static bool NativeGuardianBlocks(BeforeMurderEvent e)
    {
        if (e.IsCancelled || e.IgnoreDefense || Bypass(e.Target) || !e.Target.ProtectedByGa()) return false;
        return Block(e, e.Source, e.Target, ProtectionKind.VanillaGuardian);
    }

    public static bool Block(MiraCancelableEvent e, PlayerControl source, PlayerControl target,
        ProtectionKind kind, PlayerControl? provider = null)
    {
        if (!MayDefend(e, target)) return false;
        e.Cancel();
        // Murder requests run on every peer. Only the host publishes their result.
        // A cancelled button never creates a murder request, so its owner publishes instead.
        if (e is BeforeMurderEvent ? MarmoSession.IsHost : source.AmOwner)
            RpcBlocked(source, target, (byte)kind, provider ? provider!.PlayerId : byte.MaxValue);
        return true;
    }

    [RegisterEvent(-9900)]
    public static void BeforeMurder(BeforeMurderEvent e)
    {
        if (Bypass(e.Target)) e.IgnoreDefense = true;
        // Dirtek's unconditional protection must take priority over consumable shields.
        if (!Bypass(e.Target) && SoulguardRole.FindLivingGuard(e.Target) is { } guard)
            Block(e, e.Source, e.Target, ProtectionKind.Dirtek, guard.Player);
    }

    [MethodRpc(1140, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBlocked(PlayerControl source, PlayerControl target, byte kind, byte providerId)
    {
        if (!Enum.IsDefined(typeof(ProtectionKind), kind) || !source || !target || Bypass(target)) return;
        ShowLocal(source, target, (ProtectionKind)kind, MarmoSession.Player(providerId));
    }

    public static void ShowLocal(PlayerControl source, PlayerControl target, ProtectionKind kind,
        PlayerControl? provider = null)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !source || !target || target.HasDied()) return;
        bool entitled = source.AmOwner;
        bool knowsSource = provider && provider!.AmOwner;
        if (kind == ProtectionKind.Mirrorcaster)
        {
            var who = OptionGroupSingleton<MirrorcasterOptions>.Instance.WhoGetsNotification;
            entitled = knowsSource || source.AmOwner && who == MirrorOption.MirrorcasterAndKiller;
        }
        if (kind == ProtectionKind.Medic)
        {
            var who = OptionGroupSingleton<MedicOptions>.Instance.WhoGetsNotification;
            entitled |= who == MedicOption.Everyone ||
                target.AmOwner && who is MedicOption.Shielded or MedicOption.ShieldedAndMedic ||
                provider && provider!.AmOwner && who is MedicOption.Medic or MedicOption.ShieldedAndMedic;
            knowsSource |= entitled && !source.AmOwner;
        }
        else if (kind == ProtectionKind.Warden)
        {
            var who = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;
            bool targetKnows = target.AmOwner && who is FortifyOptions.Self or FortifyOptions.SelfAndWarden or FortifyOptions.Everyone;
            entitled |= provider && provider!.AmOwner || targetKnows;
            knowsSource |= targetKnows;
        }
        else if (kind == ProtectionKind.Cleric)
        {
            bool targetKnows = target.AmOwner && OptionGroupSingleton<ClericOptions>.Instance.ShowBarrier;
            entitled |= provider && provider!.AmOwner && OptionGroupSingleton<ClericOptions>.Instance.AttackNotif || targetKnows;
            knowsSource |= targetKnows;
        }
        else if (kind == ProtectionKind.GuardianAngel)
        {
            var o = OptionGroupSingleton<FairyOptions>.Instance;
            bool targetKnows = target.AmOwner && o.FairyTargetKnows && o.ShowProtect != ProtectOptions.Fairy;
            entitled |= knowsSource || targetKnows;
            knowsSource |= targetKnows;
        }
        else if (kind == ProtectionKind.Herbalist)
            entitled |= provider && provider!.AmOwner && OptionGroupSingleton<HerbalistOptions>.Instance.AttackNotif;
        else if (kind == ProtectionKind.Dirtek)
        {
            // A passive bond must not announce itself to the protected player.
            if (target.AmOwner && !(provider && provider!.AmOwner)) return;
            entitled |= provider && provider!.AmOwner;
        }
        else if (kind == ProtectionKind.Doctor)
        {
            entitled |= target.AmOwner || provider && provider!.AmOwner;
            knowsSource |= target.AmOwner;
        }
        else if (kind is ProtectionKind.Survivor or ProtectionKind.Invulnerable or ProtectionKind.Veteran or ProtectionKind.Monarch)
        {
            entitled |= target.AmOwner;
            knowsSource |= target.AmOwner;
        }
        if (!entitled) return;
        var key = (source.PlayerId, target.PlayerId, kind);
        if (LastFlash.TryGetValue(key, out var last) && Time.unscaledTime - last < .2f) return;
        LastFlash[key] = Time.unscaledTime;
        var color = OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields && !knowsSource
            ? TownOfUsColors.NeutralWiki : ColorFor(kind, target, provider);
        Coroutines.Start(Flash(color, ++flashGeneration));
    }

    private static Color ColorFor(ProtectionKind kind, PlayerControl target, PlayerControl? provider) => kind switch
    {
        ProtectionKind.Medic => new Color32(0, 102, 0, 255),
        ProtectionKind.Warden => new Color32(153, 0, 255, 255),
        ProtectionKind.Cleric => new Color32(0, 255, 179, 255),
        ProtectionKind.Doctor => new Color32(70, 202, 220, 255),
        ProtectionKind.Dirtek => new Color32(101, 230, 179, 255),
        ProtectionKind.GuardianAngel => TownOfUsColors.Fairy,
        ProtectionKind.VanillaGuardian => new Color32(102, 170, 243, 255),
        ProtectionKind.Survivor => TownOfUsColors.Survivor,
        ProtectionKind.FirstDead => TownOfUsColors.NeutralWiki,
        ProtectionKind.Mirrorcaster => new Color32(144, 162, 195, 255),
        ProtectionKind.Mercenary => TownOfUsColors.Mercenary,
        ProtectionKind.Monarch => new Color32(234, 83, 91, 255),
        ProtectionKind.Veteran => new Color32(153, 128, 64, 255),
        _ => provider?.Data?.Role is ITownOfUsRole r ? r.RoleColor :
            target.Data?.Role is ITownOfUsRole t ? t.RoleColor : TownOfUsColors.NeutralWiki
    };

    private static IEnumerator Flash(Color color, int generation)
    {
        // Own renderer: a different role's CoFlash cannot leave this overlay permanently enabled.
        yield return null;
        if (generation != flashGeneration || !HudManager.Instance || !HudManager.Instance.FullScreen) yield break;
        if (!overlay)
        {
            overlay = UnityEngine.Object.Instantiate(HudManager.Instance.FullScreen,
                HudManager.Instance.FullScreen.transform.parent);
            overlay.name = "MarmoProtectionFlash";
            overlay.transform.localScale *= 10f;
        }
        overlay!.gameObject.SetActive(true);
        overlay.enabled = true;
        for (float elapsed = 0; elapsed < .45f; elapsed += Time.unscaledDeltaTime)
        {
            if (generation != flashGeneration || !overlay) yield break;
            var t = elapsed / .45f;
            color.a = .28f * Mathf.Sin(Mathf.PI * t);
            overlay.color = color;
            yield return null;
        }
        if (generation == flashGeneration && overlay) overlay!.enabled = false;
    }

    public static void Clear()
    {
        BondDeaths.Clear(); LastFlash.Clear(); ++flashGeneration;
        if (overlay) UnityEngine.Object.Destroy(overlay!.gameObject);
        overlay = null;
    }
}

// Native guardian shields are independent of BaseShieldModifier.
[HarmonyPatch(typeof(MiraAPI.Utilities.Extensions), "ProtectedByGa")]
public static class DirtekNativeGuardianBypass
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl playerControl, ref bool __result)
    {
        if (!ProtectionRuntime.Bypass(playerControl)) return true;
        __result = false;
        return false;
    }
}
