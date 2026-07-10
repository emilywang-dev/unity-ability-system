using System;
using System.Collections.Generic;

namespace Gameplay.Stats
{
    /// <summary>
    /// Per-stat base and modifier list; raw evaluation before <see cref="StatCollection"/> per-type clamping.
    /// </summary>
    public sealed class Stat
    {
        private readonly List<StatModifier> _modifiers = new();

        private float? _cachedValue;

        public float BaseValue { get; private set; }

        public Stat(float baseValue)
        {
            StatValueValidation.ValidateFinite(baseValue, nameof(baseValue));
            BaseValue = baseValue;
        }

        public void SetBaseValue(float value)
        {
            StatValueValidation.ValidateFinite(value, nameof(value));
            BaseValue = value;
            InvalidateCache();
        }

        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            _modifiers.Add(modifier);
            InvalidateCache();
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            var removed = _modifiers.Remove(modifier);
            if (removed)
            {
                InvalidateCache();
            }

            return removed;
        }

        public bool RemoveModifiersFromSource(object source)
        {
            var removedCount = _modifiers.RemoveAll(modifier => ReferenceEquals(modifier.Source, source));
            if (removedCount > 0)
            {
                InvalidateCache();
                return true;
            }

            return false;
        }

        // (base + Σadditive) × (1 + Σpercent) — percents sum once, not (1+p1)×(1+p2).
        public float GetRawValue()
        {
            if (_cachedValue.HasValue)
            {
                return _cachedValue.Value;
            }

            float additive = 0f;
            float percent = 0f;

            foreach (var modifier in _modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case StatModifierType.Additive:
                        additive += modifier.Value;
                        break;

                    case StatModifierType.Percent:
                        percent += modifier.Value;
                        break;

                    default:
                        // Construction validates ModifierType; unknown values here are corrupt state.
                        throw new InvalidOperationException(
                            $"Unknown StatModifierType '{modifier.ModifierType}'.");
                }
            }

            var value = (BaseValue + additive) * (1f + percent);
            _cachedValue = value;

            return value;
        }

        private void InvalidateCache()
        {
            _cachedValue = null;
        }
    }
}
