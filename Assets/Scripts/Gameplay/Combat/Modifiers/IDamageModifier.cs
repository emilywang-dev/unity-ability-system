namespace Gameplay.Combat.Modifiers
{
    /// <summary>
    /// Single step in <see cref="DamagePipeline"/>, applied in registration order.
    /// </summary>
    /// <remarks>
    /// Order matters — register modifiers in the intended sequence at composition.
    /// </remarks>
    public interface IDamageModifier
    {
        float Modify(float damage, DamageContext context);
    }
}