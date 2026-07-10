using System;
using System.Collections.Generic;
using Core.Events;
using Gameplay.Stats.Events;

namespace Gameplay.Stats
{
    /// <summary>
    /// Stores runtime stat values and modifiers, publishing <see cref="StatChangedEvent"/> 
    /// when the clamped value changes.
    /// </summary>
    /// <remarks>
    /// Initialize each <see cref="StatType"/> via <see cref="SetBaseStat"/> 
    /// before <see cref="GetValue"/> or modifier APIs.
    /// </remarks>
    public sealed class StatCollection : IReadOnlyStatCollection
    {
        private readonly IEventBus _eventBus;
        private readonly Dictionary<StatType, Stat> _stats = new();

        public StatCollection(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void SetBaseStat(StatType statType, float baseValue)
        {
            StatValueValidation.ValidateStatType(statType, nameof(statType));
            StatValueValidation.ValidateFinite(baseValue, nameof(baseValue));

            if (_stats.TryGetValue(statType, out var stat))
            {
                var oldValue = GetComputedValue(statType, stat);
                stat.SetBaseValue(baseValue);
                PublishIfChanged(statType, oldValue, stat);
            }
            else
            {
                stat = new Stat(baseValue);
                _stats.Add(statType, stat);
                var newValue = GetComputedValue(statType, stat);
                _eventBus.Publish(new StatChangedEvent(statType, newValue, newValue));
            }
        }

        /// <inheritdoc/>
        public float GetValue(StatType statType)
        {
            if (!_stats.TryGetValue(statType, out var stat))
            {
                throw new InvalidOperationException(
                    $"Base stat '{statType}' has not been initialized.");
            }

            return GetComputedValue(statType, stat);
        }

        /// <inheritdoc/>
        public bool TryGetValue(StatType statType, out float value)
        {
            if (!_stats.TryGetValue(statType, out var stat))
            {
                value = default;
                return false;
            }

            value = GetComputedValue(statType, stat);
            return true;
        }

        /// <summary>
        /// Requires prior <see cref="SetBaseStat"/> for <see cref="StatModifier.StatType"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="StatModifier.StatType"/> was never passed to <see cref="SetBaseStat"/>.
        /// </exception>
        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            if (!_stats.TryGetValue(modifier.StatType, out var stat))
            {
                throw new InvalidOperationException(
                    $"Base stat '{modifier.StatType}' has not been initialized.");
            }

            var oldValue = GetComputedValue(modifier.StatType, stat);
            stat.AddModifier(modifier);
            PublishIfChanged(modifier.StatType, oldValue, stat);
        }

        /// <summary>
        /// False when the stat or modifier instance is absent — not an error 
        /// (contrast <see cref="RemoveModifiersFromSource"/>).
        /// </summary>
        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            if (!_stats.TryGetValue(modifier.StatType, out var stat))
            {
                return false;
            }

            var oldValue = GetComputedValue(modifier.StatType, stat);
            var removed = stat.RemoveModifier(modifier);
            if (removed)
            {
                PublishIfChanged(modifier.StatType, oldValue, stat);
            }

            return removed;
        }

        /// <summary>
        /// Removes all modifiers added by the specified source.
        /// </summary>
        /// <remarks>
        /// Designed for idempotent cleanup scenarios such as buff or ability teardown.
        /// </remarks>
        public void RemoveModifiersFromSource(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Do not mutate _stats while iterating.
            foreach (var entry in _stats)
            {
                var oldValue = GetComputedValue(entry.Key, entry.Value);
                if (!entry.Value.RemoveModifiersFromSource(source))
                {
                    continue;
                }

                PublishIfChanged(entry.Key, oldValue, entry.Value);
            }
        }

        private float GetComputedValue(StatType statType, Stat stat)
        {
            return ClampStat(statType, stat.GetRawValue());
        }

        // AttackPower/Defense/CritDamage unclamped so debuffs may go negative.
        private static float ClampStat(StatType statType, float value)
        {
            return statType switch
            {
                StatType.MaxHealth or StatType.MoveSpeed => Math.Max(value, 0f),
                StatType.CritChance => Math.Clamp(value, 0f, 1f),
                _ => value
            };
        }

        // Uses exact comparison to avoid hiding small but meaningful gameplay changes.
        private void PublishIfChanged(StatType statType, float oldValue, Stat stat)
        {
            var newValue = GetComputedValue(statType, stat);
            if (oldValue != newValue)
            {
                _eventBus.Publish(new StatChangedEvent(statType, oldValue, newValue));
            }
        }
    }
}
