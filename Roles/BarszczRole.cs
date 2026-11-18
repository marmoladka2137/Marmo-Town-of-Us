using AmongUs.GameOptions;
using TouMarmo.Assets;
using TouMarmo.Options;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TouMarmo.Roles;

public sealed class BarszczRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string IdPart => "Barszcz";

    public string RoleNameLocale => "ComposterAddon.Role.Barszcz";
    public string RoleDescriptionLocale => "ComposterAddon.Role.Barszcz.IntroBlurb";
    public string RoleMedDescriptionLocale => "ComposterAddon.Role.Barszcz.MedDescription";
    public string RoleLongDescriptionLocale => "ComposterAddon.Role.Barszcz.TabDescription";
    public string RoleWikiDescriptionLocale => "ComposterAddon.Role.Barszcz.WikiDescription";

    public Color RoleColor => BarszczColors.Role;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public RoleOptionsGroup RoleOptionsGroup => TouRoleGroups.NeutralKiller;
    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            BarszczAssets.RoleIcon.LoadAsset(), "ComposterAddon.Role.Barszcz", 1.45f),
        Icon = BarszczAssets.RoleIcon,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        IntroSound = TouAudio.WarlockIntroSound,
        CanUseVent = OptionGroupSingleton<BarszczOptions>.Instance.CanVent.Value,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get(RoleWikiDescriptionLocale) + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get("ComposterAddon.Ability.BarszczKill", "Kill"),
            MiraLocaleManager.Get("ComposterAddon.Ability.BarszczKill.WikiDescription"),
            BarszczAssets.KillButton),
    ];

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get("ComposterAddon.Ability.BarszczKill", "Kill"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get("ComposterAddon.Ability.BarszczKill.WikiDescription"),
            BarszczAssets.KillButton),
    ];

    public bool WinConditionMet()
    {
        var aliveBarszcz = CustomRoleUtils.GetActiveRolesOfType<BarszczRole>()
            .Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > aliveBarszcz || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        return aliveBarszcz >= MiscUtils.GetImpactfulLivingPlayers().Count - aliveBarszcz;
    }

    public override bool DidWin(GameOverReason gameOverReason) => WinConditionMet();

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
}
