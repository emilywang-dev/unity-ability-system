using System;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Combat.Events;
using Gameplay.Stats;
using Gameplay.Stats.Events;
using NUnit.Framework;

namespace Tests.Gameplay.Combat
{
    [TestFixture]
    public class DamageableTests
    {
        private RecordingEventBus _eventBus;
        private StatCollection _stats;
        private TestCombatEntity _entity;
        private Damageable _damageable;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new RecordingEventBus();
            _stats = new StatCollection(_eventBus);
            _stats.SetBaseStat(StatType.MaxHealth, 100f);
            _entity = new TestCombatEntity(_stats, _eventBus);
            _damageable = _entity.Damageable;
        }

        [Test]
        public void Constructor_NullOwner_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Damageable(null, _stats, _eventBus));
        }

        [Test]
        public void TakeDamage_ReducesCurrentHealthAndPublishesHealthChanged()
        {
            _damageable.TakeDamage(30f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(70f).Within(0.001f));
            Assert.AreEqual(1, _eventBus.HealthChangedEvents.Count);
            Assert.AreSame(_entity, _eventBus.HealthChangedEvents[0].Entity);
            Assert.That(_eventBus.HealthChangedEvents[0].CurrentHealth, Is.EqualTo(70f).Within(0.001f));
            Assert.That(_eventBus.HealthChangedEvents[0].MaxHealth, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Constructor_NullStats_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Damageable(_entity, null, _eventBus));
        }

        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Damageable(_entity, _stats, null));
        }

        [Test]
        public void TakeDamage_ZeroAmount_NoOpsWithoutPublishing()
        {
            _damageable.TakeDamage(0f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(100f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
        }

        [Test]
        public void TakeDamage_DoesNotPublishDamageEvent()
        {
            _damageable.TakeDamage(10f);

            Assert.IsEmpty(_eventBus.DamageEvents);
        }

        [Test]
        public void TakeDamage_NegativeAmount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _damageable.TakeDamage(-1f));
        }

        [Test]
        public void TakeDamage_ExceedsCurrentHealth_ClampsToZero()
        {
            _damageable.TakeDamage(150f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.IsTrue(_damageable.IsDead);
        }

        [Test]
        public void TakeDamage_WhenDead_NoOpsWithoutPublishing()
        {
            _damageable.TakeDamage(100f);
            _eventBus.HealthChangedEvents.Clear();

            _damageable.TakeDamage(10f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
        }

        [Test]
        public void MaxHealth_ReadsLiveStatCollectionValue()
        {
            _stats.AddModifier(new StatModifier(
                StatType.MaxHealth,
                StatModifierType.Additive,
                50f,
                new object()));

            Assert.That(_damageable.MaxHealth, Is.EqualTo(150f).Within(0.001f));
        }

        [Test]
        public void ClampCurrentHealthToMax_WhenMaxHealthDecreases_ClampsAndPublishes()
        {
            _damageable.TakeDamage(40f);
            _eventBus.HealthChangedEvents.Clear();

            _stats.AddModifier(new StatModifier(
                StatType.MaxHealth,
                StatModifierType.Additive,
                -50f,
                new object()));

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(50f).Within(0.001f));
            Assert.AreEqual(1, _eventBus.HealthChangedEvents.Count);
            Assert.That(_eventBus.HealthChangedEvents[0].CurrentHealth, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void StatChanged_NonMaxHealth_DoesNotClampCurrentHealth()
        {
            _damageable.TakeDamage(20f);

            _stats.SetBaseStat(StatType.AttackPower, 10f);
            _stats.AddModifier(new StatModifier(
                StatType.AttackPower,
                StatModifierType.Additive,
                10f,
                new object()));

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(80f).Within(0.001f));
        }

        [Test]
        public void Heal_IncreasesCurrentHealthAndPublishesHealthChangedAndHealEvent()
        {
            _damageable.TakeDamage(40f);
            _eventBus.Clear();

            _damageable.Heal(25f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(85f).Within(0.001f));
            Assert.AreEqual(1, _eventBus.HealthChangedEvents.Count);
            Assert.AreEqual(1, _eventBus.HealEvents.Count);
            Assert.That(_eventBus.HealEvents[0].Amount, Is.EqualTo(25f).Within(0.001f));
            Assert.AreSame(_entity, _eventBus.HealEvents[0].Entity);
        }

        [Test]
        public void Heal_NegativeAmount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _damageable.Heal(-1f));
        }

        [Test]
        public void Heal_ZeroAmount_NoOpsWithoutPublishing()
        {
            _damageable.TakeDamage(20f);
            _eventBus.Clear();

            _damageable.Heal(0f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(80f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
            Assert.IsEmpty(_eventBus.HealEvents);
        }

        [Test]
        public void Heal_WhenDead_NoOpsWithoutPublishing()
        {
            _damageable.TakeDamage(100f);
            _eventBus.Clear();

            _damageable.Heal(50f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
            Assert.IsEmpty(_eventBus.HealEvents);
        }

        [Test]
        public void Heal_AtMaxHealth_NoOpsWithoutPublishing()
        {
            _damageable.Heal(25f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(100f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
            Assert.IsEmpty(_eventBus.HealEvents);
        }

        [Test]
        public void Heal_ClampsToLiveMaxHealth()
        {
            _damageable.TakeDamage(30f);
            _eventBus.Clear();

            _damageable.Heal(50f);

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(100f).Within(0.001f));
            Assert.That(_eventBus.HealEvents[0].Amount, Is.EqualTo(30f).Within(0.001f));
        }

        [Test]
        public void Heal_PublishesHealthChangedBeforeHealEvent()
        {
            _damageable.TakeDamage(50f);
            _eventBus.Clear();

            _damageable.Heal(10f);

            Assert.AreEqual(2, _eventBus.PublishedEventTypes.Count);
            Assert.AreEqual(typeof(HealthChangedEvent), _eventBus.PublishedEventTypes[0]);
            Assert.AreEqual(typeof(HealEvent), _eventBus.PublishedEventTypes[1]);
        }

        [Test]
        public void ClampCurrentHealthToMax_WhenCurrentAlreadyWithinMax_DoesNotPublish()
        {
            _damageable.TakeDamage(80f);
            _eventBus.HealthChangedEvents.Clear();

            _stats.AddModifier(new StatModifier(
                StatType.MaxHealth,
                StatModifierType.Additive,
                -10f,
                new object()));

            Assert.That(_damageable.CurrentHealth, Is.EqualTo(20f).Within(0.001f));
            Assert.IsEmpty(_eventBus.HealthChangedEvents);
        }

        [Test]
        public void Heal_DoesNotPublishDamageEvent()
        {
            _damageable.TakeDamage(20f);
            _eventBus.Clear();

            _damageable.Heal(10f);

            Assert.IsEmpty(_eventBus.DamageEvents);
        }
    }
}
