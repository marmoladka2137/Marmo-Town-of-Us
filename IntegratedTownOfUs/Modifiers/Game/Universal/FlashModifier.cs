using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class FlashModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Flash,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Flash.LoadAsset(),
            "TouMira.Modifier.Universal.Flash", 1.45f));
    public override string IdPart => "Flash";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Flash;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed;
        return appearance;
    }

    public override string ModifierMedDescription => GetDescription();
    public override string GetDescription() => FormatSpeed(MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription"));

    private static string FormatSpeed(string text)
    {
        var speed = OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        return text.Contains("<flashSpeed>") || text.Contains("[flashSpeed]")
            ? text.Replace("<flashSpeed>", speed).Replace("[flashSpeed]", speed)
            : $"{text} ({speed}x)";
    }

    public string GetAdvancedDescription()
    {
        return FormatSpeed(MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription")) +
               MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FlashAmount;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance(fullReset: true);
    }
}
