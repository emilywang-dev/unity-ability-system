using Gameplay.Stats;

namespace Gameplay.Combat.Modifiers
{
    /// <summary>
    /// Applies Defense mitigation; skips <see cref="DamageType.True"/>.
    /// </summary>
    public sealed class DefenseModifier : IDamageModifier
    {
        private const float DefenseConstant = 100f;

        public float Modify(float damage, DamageContext context)
        {
            if (context.DamageType == DamageType.True)
            {
                return damage;
            }

            float defense = context.Target.Stats.GetValue(StatType.Defense);
            float denominator = DefenseConstant + defense;
            // Defense may be negative (unclamped in Stats); avoid divide-by-zero or negative denominators.
            if (denominator <= 0f)
            {
                return damage;
            }

            return damage * DefenseConstant / denominator;
        }
    }
}
