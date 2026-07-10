using System;

namespace Gameplay.Stats
{
    // Internal validation helpers; not part of the public Stats API surface.
    internal static class StatValueValidation
    {
        internal static void ValidateStatType(StatType statType, string paramName)
        {
            if (statType == StatType.None)
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    statType,
                    "StatType must not be None.");
            }
        }

        internal static void ValidateFinite(float value, string paramName)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    "Stat value must be finite.");
            }
        }

        internal static void ValidateModifierType(StatModifierType modifierType, string paramName)
        {
            if (modifierType is not (StatModifierType.Additive or StatModifierType.Percent))
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    modifierType,
                    "Unknown StatModifierType.");
            }
        }
    }
}
