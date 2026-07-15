using Core.Events;
using Gameplay.Combat;

namespace Gameplay.Combat.Events
{
    /// <summary>
    /// Post-pipeline damage attribution; amount is non-negative.
    /// </summary>
    public readonly struct DamageEvent : IEvent
    {
        public DamageEvent(
            ICombatEntity source,
            ICombatEntity target,
            DamageType damageType,
            float amount,
            bool isCrit)
        {
            Source = source;
            Target = target;
            DamageType = damageType;
            Amount = amount;
            IsCrit = isCrit;
        }

        public ICombatEntity Source { get; }

        public ICombatEntity Target { get; }

        public DamageType DamageType { get; }

        public float Amount { get; }

        public bool IsCrit { get; }
    }
}