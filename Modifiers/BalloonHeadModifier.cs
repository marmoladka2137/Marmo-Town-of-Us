using TouMarmo.Assets;
using TouMarmo.Options;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modifiers;

// A network-synced visual effect, not a modifier distributed at game start.
public sealed class BalloonHeadModifier : BaseModifier
{
    private GameObject? _balloon;
    private float _remainingSeconds;
    private bool _expiring;

    public override string ModifierName => "Ballooned";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        _remainingSeconds = OptionGroupSingleton<TouMarmo.Options.UniversalModifiersOptions>.Instance.BalloonDuration.Value;
        _expiring = false;
        if (!Player || _balloon != null)
        {
            return;
        }

        _balloon = new GameObject($"Balloon_{Player.PlayerId}");
        _balloon.transform.SetParent(Player.transform, false);
        _balloon.transform.localPosition = new Vector3(0f, 1.35f, -0.2f);
        var renderer = _balloon.AddComponent<SpriteRenderer>();
        renderer.sprite = BalloonAssets.Head.LoadAsset();
        renderer.sortingOrder = 30;
    }

    public override void Update()
    {
        if (_expiring)
        {
            return;
        }

        _remainingSeconds -= Time.deltaTime;
        if (_remainingSeconds <= 0f)
        {
            _expiring = true;
            ModifierComponent?.RemoveModifier(this);
        }
    }

    public override void OnDeactivate()
    {
        if (_balloon != null)
        {
            Object.Destroy(_balloon);
            _balloon = null;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
