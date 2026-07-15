using Gameplay.Stats;

namespace Gameplay.Combat.Modifiers
{
    /// <summary>
    /// Applies <see cref="StatType.CritDamage"/> on crit, including <see cref="DamageType.True"/>.
    /// </summary>
    public sealed class CritModifier : IDamageModifier
    {
        public float Modify(float damage, DamageContext context)
        {
            if (!context.IsCrit)
            {
                return damage;
            }

            return damage * context.Source.Stats.GetValue(StatType.CritDamage);
        }
    }
}