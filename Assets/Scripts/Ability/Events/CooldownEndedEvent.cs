using Core.Events;

namespace Ability.Events
{
    /// <summary>
    /// Published on the owning entity's local bus when an ability's cooldown reaches zero.
    /// </summary>
    public readonly struct CooldownEndedEvent : IEvent
    {
        public readonly int AbilityIndex;

        public CooldownEndedEvent(int abilityIndex)
        {
            AbilityIndex = abilityIndex;
        }
    }
}
