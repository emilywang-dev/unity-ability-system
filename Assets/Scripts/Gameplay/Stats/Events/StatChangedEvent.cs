using Core.Events;
using Gameplay.Stats;

namespace Gameplay.Stats.Events
{
    /// /// <summary>
    /// Notifies listeners when a stat's clamped computed value is initialized or changes.
    /// </summary>
    /// <remarks>
    /// Values are <see cref="StatCollection"/>-clamped computed values, not raw <see cref="Stat.GetRawValue"/> output.
    /// Initialization uses <see cref="OldValue"/> == <see cref="NewValue"/> 
    /// so subscribers need not special-case the first publish.
    /// </remarks>
    public readonly struct StatChangedEvent : IEvent
    {
        public readonly StatType StatType;
        public readonly float OldValue;
        public readonly float NewValue;

        public StatChangedEvent(StatType statType, float oldValue, float newValue)
        {
            StatType = statType;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
