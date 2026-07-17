using Core.Events;

namespace Ability.Events
{
    /// <summary>
    /// Published on the owning entity's local bus when an ability's cooldown begins.
    /// </summary>
    public readonly struct CooldownStartedEvent : IEvent
    {
        public readonly int AbilityIndex;
        public readonly float Duration;

        public CooldownStartedEvent(int abilityIndex, float duration)
        {
            AbilityIndex = abilityIndex;
            Duration = duration;
        }
    }
}
