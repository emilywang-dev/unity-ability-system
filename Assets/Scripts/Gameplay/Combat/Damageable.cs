using System;
using Core.Events;
using Gameplay.Combat.Events;
using Gameplay.Stats;
using Gameplay.Stats.Events;

namespace Gameplay.Combat
{
    /// <summary>
    /// Tracks current health for a combat entity and publishes local health notifications.
    /// </summary>
    /// <remarks>
    /// The injected event bus must match the entity's <see cref="StatCollection"/>; a mismatched bus
    /// silently breaks MaxHealth clamp. One bus per entity — do not reuse across
    /// <see cref="Damageable"/> instances. Subscribes to <see cref="StatChangedEvent"/> in the ctor
    /// and never unsubscribes; safe because bus and this instance share the same lifetime.
    /// </remarks>
    public sealed class Damageable
    {
        private readonly ICombatEntity _owner;
        private readonly IReadOnlyStatCollection _stats;
        private readonly IEventBus _eventBus;

        public Damageable(
            ICombatEntity owner,
            IReadOnlyStatCollection stats,
            IEventBus eventBus)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            CurrentHealth = MaxHealth;

            _eventBus.Subscribe<StatChangedEvent>(OnStatChanged);
        }

        public float CurrentHealth { get; private set; }

        public float MaxHealth => _stats.GetValue(StatType.MaxHealth);

        public bool IsDead => CurrentHealth <= 0f;

        /// <summary>
        /// Applies damage and publishes <see cref="HealthChangedEvent"/>; does not publish
        /// <see cref="DamageEvent"/> — that is <see cref="DamagePipeline"/>'s job. Also used by DoT
        /// and other paths that bypass the pipeline.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="damage"/> is negative.</exception>
        public void TakeDamage(float damage)
        {
            if (damage < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damage),
                    damage,
                    "Damage amount must be non-negative.");
            }

            if (damage == 0f || IsDead)
            {
                return;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = MathF.Max(0f, CurrentHealth - damage);
            if (CurrentHealth != previousHealth)
            {
                PublishHealthChanged();
            }
        }

        /// <summary>
        /// Escape hatch when max health changes outside the <see cref="StatChangedEvent"/> path.
        /// </summary>
        /// <remarks>
        /// Normal path: <see cref="StatChangedEvent"/> on <c>StatType.MaxHealth</c>.
        /// Not for buff/combat callers.
        /// </remarks>
        public void ClampCurrentHealthToMax()
        {
            float maxHealth = MaxHealth;
            if (CurrentHealth <= maxHealth)
            {
                return;
            }

            CurrentHealth = maxHealth;
            PublishHealthChanged();
        }

        /// <summary>
        /// Applies healing and publishes <see cref="HealthChangedEvent"/> then <see cref="HealEvent"/>.
        /// No-op when dead — revival is a separate system.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
        public void Heal(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "Heal amount must be non-negative.");
            }

            if (amount == 0f || IsDead)
            {
                return;
            }

            float maxHealth = MaxHealth;
            float previousHealth = CurrentHealth;
            CurrentHealth = MathF.Min(maxHealth, CurrentHealth + amount);

            float healedAmount = CurrentHealth - previousHealth;
            if (healedAmount == 0f)
            {
                return;
            }

            PublishHealthChanged();
            _eventBus.Publish(new HealEvent(_owner, healedAmount));
        }

        private void OnStatChanged(StatChangedEvent statChanged)
        {
            if (statChanged.StatType == StatType.MaxHealth)
            {
                ClampCurrentHealthToMax();
            }
        }

        private void PublishHealthChanged()
        {
            _eventBus.Publish(new HealthChangedEvent(_owner, CurrentHealth, MaxHealth));
        }
    }
}
