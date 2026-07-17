namespace Gameplay.Combat.Modifiers
{
    /// <summary>
    /// One step that transforms damage for a hit.
    /// </summary>
    /// <remarks>
    /// Order matters — register modifiers in the intended sequence at composition.
    /// </remarks>
    public interface IDamageModifier
    {
        float Modify(float damage, DamageContext context);
    }
}
