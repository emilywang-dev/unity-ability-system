using System;
using System.Collections.Generic;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Combat.Events;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using Gameplay.Stats.Events;

namespace Tests.Gameplay.Combat
{
    internal sealed class TestCombatEntity : ICombatEntity
    {
        public TestCombatEntity(IReadOnlyStatCollection stats)
            : this(stats, new RecordingEventBus())
        {
        }

        public TestCombatEntity(IReadOnlyStatCollection stats, IEventBus eventBus)
        {
            Stats = stats;
            LocalEventBus = eventBus as RecordingEventBus;
            Damageable = new Damageable(this, stats, eventBus);
        }

        public IReadOnlyStatCollection Stats { get; }

        public Damageable Damageable { get; }

        // Non-null only when constructed with RecordingEventBus; null for NoOpEventBus.
        public RecordingEventBus LocalEventBus { get; }
    }

    internal sealed class NoOpEventBus : IEventBus
    {
        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
        }

        public void Publish<T>(T evt) where T : IEvent
        {
        }
    }

    // Test double: records published events for assertions. StatChangedEvent keeps only the
    // last subscriber — intentional simplification; use one bus per entity (see DamageableTests).
    internal sealed class RecordingEventBus : IEventBus
    {
        public List<HealthChangedEvent> HealthChangedEvents { get; } = new();

        public List<HealEvent> HealEvents { get; } = new();

        public List<DamageEvent> DamageEvents { get; } = new();

        public List<Type> PublishedEventTypes { get; } = new();

        public Action<StatChangedEvent> StatChangedHandler { get; private set; }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler is Action<StatChangedEvent> statHandler)
            {
                StatChangedHandler = statHandler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
        }

        public void Clear()
        {
            HealthChangedEvents.Clear();
            HealEvents.Clear();
            DamageEvents.Clear();
            PublishedEventTypes.Clear();
        }

        public void Publish<T>(T evt) where T : IEvent
        {
            PublishedEventTypes.Add(typeof(T));

            if (evt is HealthChangedEvent healthChanged)
            {
                HealthChangedEvents.Add(healthChanged);
            }
            else if (evt is HealEvent healEvent)
            {
                HealEvents.Add(healEvent);
            }
            else if (evt is DamageEvent damageEvent)
            {
                DamageEvents.Add(damageEvent);
            }
            else if (evt is StatChangedEvent statChanged)
            {
                StatChangedHandler?.Invoke(statChanged);
            }
        }
    }

    internal sealed class FixedValueModifier : IDamageModifier
    {
        private readonly float _value;

        public FixedValueModifier(float value)
        {
            _value = value;
        }

        public float Modify(float damage, DamageContext context)
        {
            return _value;
        }
    }

    internal sealed class OrderRecordingModifier : IDamageModifier
    {
        private readonly List<string> _log;
        private readonly string _name;

        public OrderRecordingModifier(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        public float Modify(float damage, DamageContext context)
        {
            _log.Add(_name);
            return damage;
        }
    }
}
