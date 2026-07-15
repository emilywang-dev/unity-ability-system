using Core.Events;
using Gameplay.Combat;

namespace Gameplay.Combat.Events
{
    /// <summary>
    /// Applied heal amount (post-clamp); not published for no-ops.
    /// </summary>
    /// <remarks>
    /// <c>Amount</c> is the applied heal (after clamping to max health), not the requested amount.
    /// Always positive: no-op heals do not publish this event.
    /// </remarks>
    public readonly struct HealEvent : IEvent
    {
        public HealEvent(ICombatEntity entity, float amount)
        {
            Entity = entity;
            Amount = amount;
        }

        public ICombatEntity Entity { get; }

        public float Amount { get; }
    }
}
