using System;
using System.Collections.Generic;
using Core.Events;
using Gameplay.Combat.Events;
using Gameplay.Combat.Modifiers;

namespace Gameplay.Combat
{
    /// <summary>
    /// Shared damage calculation pipeline applying a registered modifier chain.
    /// </summary>
    /// <remarks>
    /// Construct once and share — fixed modifier list only; no per-execution or per-entity state.
    /// </remarks>
    public sealed class DamagePipeline
    {
        private readonly IReadOnlyList<IDamageModifier> _modifiers;

        public DamagePipeline(IReadOnlyList<IDamageModifier> modifiers)
        {
            _modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        /// <summary>
        /// Runs <paramref name="damage"/> through the modifier chain in registration order,
        /// applies it via <see cref="Damageable.TakeDamage"/>, then publishes <see cref="DamageEvent"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="targetEventBus"/> is per-call (typically from <c>AbilityTargetDeps</c>),
        /// not ctor-fixed — <see cref="Damageable"/> does not expose its bus, and this pipeline
        /// is shared across entities.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="damage"/> is negative.</exception>
        public void Execute(float damage, DamageContext context, IEventBus targetEventBus)
        {
            if (damage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            if (targetEventBus == null)
            {
                throw new ArgumentNullException(nameof(targetEventBus));
            }

            // Skip before modifiers — TakeDamage's IsDead guard only skips health mutation.
            if (context.Target.Damageable.IsDead)
            {
                return;
            }

            float finalDamage = damage;
            foreach (IDamageModifier modifier in _modifiers)
            {
                finalDamage = modifier.Modify(finalDamage, context);
            }
            finalDamage = MathF.Max(0f, finalDamage);

            context.Target.Damageable.TakeDamage(finalDamage);

            targetEventBus.Publish(
                new DamageEvent(
                    context.Source,
                    context.Target,
                    context.DamageType,
                    finalDamage,
                    context.IsCrit));
        }
    }
}
