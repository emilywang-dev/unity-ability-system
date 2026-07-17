using System;
using System.Collections.Generic;
using NUnit.Framework;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Combat.Events;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;

namespace Tests.Gameplay.Combat
{
    [TestFixture]
    public class DamagePipelineTests
    {
        private RecordingEventBus _targetEventBus;
        private StatCollection _sourceStats;
        private StatCollection _targetStats;
        private TestCombatEntity _source;
        private TestCombatEntity _target;
        private DamagePipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _targetEventBus = new RecordingEventBus();
            var sourceBus = new RecordingEventBus();
            var targetBus = new RecordingEventBus();
            _sourceStats = CreateStats(sourceBus, 100f, defense: 0f, critDamage: 2f);
            _targetStats = CreateStats(targetBus, 100f, defense: 100f, critDamage: 1.5f);
            _source = new TestCombatEntity(_sourceStats, sourceBus);
            _target = new TestCombatEntity(_targetStats, targetBus);
            _pipeline = new DamagePipeline(new IDamageModifier[]
            {
                new DefenseModifier(),
                new CritModifier()
            });
        }

        [Test]
        public void Execute_PublishesDamageEventOnTargetBus_NotFromTakeDamageAlone()
        {
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            _pipeline.Execute(100f, context, _targetEventBus);

            Assert.AreEqual(1, _targetEventBus.DamageEvents.Count);
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(50f).Within(0.001f));
            Assert.IsEmpty(_target.LocalEventBus.DamageEvents);
        }

        [Test]
        public void Execute_AppliesDefenseAndCritBeforeTakeDamage()
        {
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: true);

            _pipeline.Execute(100f, context, _targetEventBus);

            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(100f).Within(0.001f));
            Assert.IsTrue(_targetEventBus.DamageEvents[0].IsCrit);
        }

        [Test]
        public void Execute_TrueDamage_SkipsDefenseButStillCrits()
        {
            var context = CreateContext(_source, _target, DamageType.True, isCrit: true);

            _pipeline.Execute(50f, context, _targetEventBus);

            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Execute_PublishesHealthChangedOnTargetLocalBus()
        {
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            _pipeline.Execute(30f, context, _targetEventBus);

            Assert.AreEqual(1, _target.LocalEventBus.HealthChangedEvents.Count);
            Assert.That(_target.LocalEventBus.HealthChangedEvents[0].CurrentHealth, Is.EqualTo(85f).Within(0.001f));
            Assert.IsEmpty(_target.LocalEventBus.DamageEvents);
        }

        [Test]
        public void Execute_PublishesDamageEventWithContextFields()
        {
            var context = CreateContext(_source, _target, DamageType.Magical, isCrit: true);

            _pipeline.Execute(100f, context, _targetEventBus);

            var damageEvent = _targetEventBus.DamageEvents[0];
            Assert.AreSame(_source, damageEvent.Source);
            Assert.AreSame(_target, damageEvent.Target);
            Assert.AreEqual(DamageType.Magical, damageEvent.DamageType);
            Assert.That(damageEvent.Amount, Is.EqualTo(100f).Within(0.001f));
            Assert.IsTrue(damageEvent.IsCrit);
        }

        [Test]
        public void Execute_CallsModifiersInRegistrationOrder()
        {
            var log = new List<string>();
            var pipeline = new DamagePipeline(new IDamageModifier[]
            {
                new OrderRecordingModifier(log, "First"),
                new OrderRecordingModifier(log, "Second"),
            });
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            pipeline.Execute(10f, context, _targetEventBus);

            Assert.AreEqual(new[] { "First", "Second" }, log);
        }

        [Test]
        public void Execute_ClampsNegativeFinalDamageToZero()
        {
            var pipeline = new DamagePipeline(new IDamageModifier[]
            {
                new FixedValueModifier(-999f)
            });
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            pipeline.Execute(10f, context, _targetEventBus);

            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(100f).Within(0.001f));
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Execute_WhenTargetDead_ShortCircuitsWithoutPublishing()
        {
            _target.Damageable.TakeDamage(100f);
            _targetEventBus.DamageEvents.Clear();

            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);
            _pipeline.Execute(50f, context, _targetEventBus);

            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(0f).Within(0.001f));
            Assert.IsEmpty(_targetEventBus.DamageEvents);
        }

        [Test]
        public void Execute_NullTargetEventBus_ThrowsArgumentNullException()
        {
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            Assert.Throws<ArgumentNullException>(() =>
                _pipeline.Execute(10f, context, null));
        }

        [Test]
        public void Constructor_NullModifiers_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DamagePipeline(null));
        }

        [Test]
        public void Execute_NegativeDamage_ThrowsArgumentOutOfRangeException()
        {
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _pipeline.Execute(-1f, context, _targetEventBus));
        }

        [Test]
        public void Execute_EmptyModifierChain_AppliesRawDamageAndPublishesDamageEvent()
        {
            var pipeline = new DamagePipeline(Array.Empty<IDamageModifier>());
            var context = CreateContext(_source, _target, DamageType.Physical, isCrit: false);

            pipeline.Execute(25f, context, _targetEventBus);

            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(75f).Within(0.001f));
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(25f).Within(0.001f));
        }

        private static StatCollection CreateStats(
            IEventBus bus,
            float maxHealth,
            float defense,
            float critDamage)
        {
            var stats = new StatCollection(bus);
            stats.SetBaseStat(StatType.MaxHealth, maxHealth);
            stats.SetBaseStat(StatType.Defense, defense);
            stats.SetBaseStat(StatType.CritDamage, critDamage);
            return stats;
        }

        private static DamageContext CreateContext(
            ICombatEntity source,
            ICombatEntity target,
            DamageType damageType,
            bool isCrit)
        {
            return new DamageContext(source, target, damageType, isCrit);
        }
    }
}
