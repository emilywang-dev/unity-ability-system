using System;

namespace Gameplay.Stats
{
    /// <summary>
    /// Immutable contribution to one <see cref="StatType"/>, keyed by <c>Source</c> for batch removal.
    /// </summary>
    public sealed class StatModifier
    {
        public StatType StatType { get; }

        public StatModifierType ModifierType { get; }

        public float Value { get; }

        /// <summary>
        /// Reference-equality owner for <see cref="StatCollection.RemoveModifiersFromSource"/> —
        /// pass the applying instance (e.g. buff instance), not a shared config, or Independent stacking
        /// cannot drop one instance without clearing siblings.
        /// </summary>
        public object Source { get; }

        public StatModifier(StatType statType, StatModifierType modifierType, float value, object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            StatValueValidation.ValidateStatType(statType, nameof(statType));
            StatValueValidation.ValidateModifierType(modifierType, nameof(modifierType));
            StatValueValidation.ValidateFinite(value, nameof(value));

            StatType = statType;
            ModifierType = modifierType;
            Value = value;
            Source = source;
        }
    }
}
