using Core.Events;
using Gameplay.Combat;

namespace Gameplay.Combat.Events
{
    /// <summary>
    /// Global-bus death signal from dead-state Enter — not from <see cref="Damageable"/>.
    /// </summary>
    public readonly struct EntityDiedEvent : IEvent
    {
        public EntityDiedEvent(ICombatEntity entity)
        {
            Entity = entity;
        }

        public ICombatEntity Entity { get; }
    }
}