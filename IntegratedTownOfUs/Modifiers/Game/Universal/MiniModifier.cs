using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class MiniModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Mini,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Mini.LoadAsset(),
            "TouMira.Modifier.Universal.Mini", 1.45f));
    public override string IdPart => "Mini";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Mini;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed;
        appearance.Size = new Vector3(0.49f, 0.49f, 1f);
        return appearance;
    }

    public override string ModifierMedDescription => GetDescription();
    public override string GetDescription() => FormatSpeed(MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription"));

    private static string FormatSpeed(string text)
    {
        var speed = OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        return text.Contains("<miniSpeed>") || text.Contains("[miniSpeed]")
            ? text.Replace("<miniSpeed>", speed).Replace("[miniSpeed]", speed)
            : $"{text} ({speed}x)";
    }

    public string GetAdvancedDescription()
    {
        return FormatSpeed(MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription"));
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount;
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
