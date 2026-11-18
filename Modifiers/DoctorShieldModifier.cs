using MiraAPI.Modifiers;
namespace TouMarmo.Modifiers;
public sealed class DoctorShieldModifier : BaseModifier
{
    public override string ModifierName => "Doctor Shield";
    public override bool HideOnUi => true;
    public override void OnDeath(DeathReason reason) => ModifierComponent?.RemoveModifier(this);
}
