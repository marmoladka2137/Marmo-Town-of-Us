using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TouMarmo.Assets;
using TouMarmo.Modules;
using TouMarmo.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Patches.Options;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class SoulguardRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole
{
    // A target discovers the private channel only if the guard deliberately starts a chat.
    private static readonly HashSet<byte> OpenedChats = new();
    public static void ClearChatKnowledge() => OpenedChats.Clear();
    public string IdPart => "Soulguard";
    public string RoleNameLocale => "TouMarmo.Role.Soulguard";
    public string RoleDescriptionLocale => "TouMarmo.Role.Soulguard.IntroBlurb";
    public string RoleMedDescriptionLocale => "TouMarmo.Role.Soulguard.MedDescription";
    public string RoleLongDescriptionLocale => "TouMarmo.Role.Soulguard.TabDescription";
    public string RoleWikiDescriptionLocale => "TouMarmo.Role.Soulguard.WikiDescription";
    public Color RoleColor => new Color32(101, 230, 179, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.NeutralBenign;
    public byte ProtectedPlayerId { get; internal set; } = byte.MaxValue;
    public bool BondBreaking { get; set; }
    public PlayerControl? ProtectedPlayer => PlayerControl.AllPlayerControls.ToArray()
        .FirstOrDefault(p => p.PlayerId == ProtectedPlayerId);

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MarmoAssets.Soulguard,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(MarmoAssets.Soulguard.LoadAsset(), "TouMarmo.Role.Soulguard", 1.45f),
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProtectedPlayerId = byte.MaxValue;
        BondBreaking = false;
    }

    public override void Deinitialize(PlayerControl player)
    {
        base.Deinitialize(player);
        ProtectedPlayerId = byte.MaxValue;
        BondBreaking = false;
    }

    public override bool DidWin(GameOverReason reason) => !Player.HasDied() &&
        ProtectedPlayer is { } protectedPlayer && !protectedPlayer.HasDied();

    [MethodRpc((uint)MarmoRpc.Protect, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcProtect(PlayerControl guard, PlayerControl target)
    {
        if (LobbyBehaviour.Instance || MeetingHud.Instance || guard.Data.Role is not SoulguardRole role ||
            guard.HasDied() || target.HasDied() || guard == target ||
            role.ProtectedPlayerId != byte.MaxValue)
        {
            return;
        }

        role.ProtectedPlayerId = target.PlayerId;
        DirtekBondRuntime.Bind(guard, target);
        if (guard.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TouMarmo.Role.Soulguard.GuardingNotice", "You are protecting <player>.")
                    .Replace("<player>", target.Data.PlayerName),
                Color.white, new Vector3(0f, 1f, -20f), spr: MarmoAssets.Protect.LoadAsset());
        }
    }

    public static SoulguardRole? FindLivingGuard(PlayerControl target) =>
        PlayerControl.AllPlayerControls.ToArray()
            .Select(p => p.Data?.Role)
            .OfType<SoulguardRole>()
            .FirstOrDefault(r => r.Player && r.Player.Data != null && !r.Player.Data.Disconnected && !r.Player.HasDied() && !r.BondBreaking &&
                r.ProtectedPlayerId == target.PlayerId);

    public static void AssignPermanentTargets()
    {
        if (!MarmoSession.IsHost) return;
        foreach (var guard in PlayerControl.AllPlayerControls.ToArray())
        {
            if (guard.Data?.Role is not SoulguardRole role || guard.HasDied() ||
                role.ProtectedPlayerId != byte.MaxValue) continue;
            var candidates = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => p != guard && p.Data != null && !p.Data.Disconnected &&
                    !p.HasDied() && p.Data.Role is not SoulguardRole).ToArray();
            if (candidates.Length > 0)
                RpcProtect(guard, candidates[System.Random.Shared.Next(candidates.Length)]);
        }
    }

    public static bool LocalCanChat()
    {
        if (!MeetingHud.Instance || !PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.HasDied() ||
            !OptionGroupSingleton<SoulguardOptions>.Instance.PrivateChat.Value)
        {
            return false;
        }

        var local = PlayerControl.LocalPlayer;
        return local.Data.Role is SoulguardRole { ProtectedPlayerId: not byte.MaxValue } guard &&
                   !guard.Player.HasDied() && guard.ProtectedPlayer is { } protectedPlayer && !protectedPlayer.HasDied();
    }

    [MethodRpc((uint)MarmoRpc.BondChat)]
    public static void RpcBondChat(PlayerControl sender, string text)
    {
        if (!MeetingHud.Instance || string.IsNullOrWhiteSpace(text) || text.Length > 300 ||
            !OptionGroupSingleton<SoulguardOptions>.Instance.PrivateChat.Value)
        {
            return;
        }

        var guard = sender.Data.Role as SoulguardRole;
        if (guard == null || guard.ProtectedPlayer is not { } protectedPlayer ||
            sender != guard.Player || guard.Player.HasDied() || protectedPlayer.HasDied())
        {
            return;
        }


        var local = PlayerControl.LocalPlayer;
        if (local == guard.Player)
        {
            MiscUtils.AddTeamChat(sender.Data, "<color=#65E6B3>Dirtek</color>", text,
                !sender.AmOwner, blackoutText: false, bubbleType: BubbleType.Jailor,
                onLeft: !sender.AmOwner);
            if (!sender.AmOwner)
            {
                TeamChatPatches.TeamChatManager.MarkChatAsUnread(60);
            }
        }
    }
}
