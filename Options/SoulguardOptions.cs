// Keep the exact option object shared by the UI, configuration, RPC sync and gameplay.
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TouMarmo.Roles;

namespace TouMarmo.Options;

public sealed class SoulguardOptions : AbstractRoleOptionGroup<SoulguardRole>
{
    public override string GroupName => MiraLocaleManager.Get("TouMarmo.Role.Soulguard", "Dirtek");

    public ModdedToggleOption PrivateChat { get; } = new("TouMarmo.Option.PrivateChat", true);

}

