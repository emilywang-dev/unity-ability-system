using Core.Events;
using Gameplay.Combat;

namespace Gameplay.Combat.Events
{
    /// <summary>
    /// Snapshot of current/max health after a change (entity local bus).
    /// </summary>
    /// <remarks>
    /// Snapshot-only (<c>CurrentHealth</c> / <c>MaxHealth</c>) — no cause or delta by design.
    /// Damage, heal, and max-health clamp all publish this shape.
    /// </remarks>
    public readonly struct HealthChangedEvent : IEvent
    {
        public HealthChangedEvent(
            ICombatEntity entity,
            float currentHealth,
            float maxHealth)
        {
            Entity = entity;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        public ICombatEntity Entity { get; }

        public float CurrentHealth { get; }

        public float MaxHealth { get; }
    }
}
