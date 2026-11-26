namespace TouMarmo.Modules;
// Zero is the option's infinity endpoint. Keep serialized values finite and deterministic.
public static class AbilityLimits
{
    public static int Count(int value) => value <= 0 ? int.MaxValue : value;
    public static int Count(float value) => value <= 0 ? int.MaxValue : (int)value;
    public static int Count(MiraAPI.GameOptions.OptionTypes.ModdedNumberOption option) => Count(option.Value);
}
