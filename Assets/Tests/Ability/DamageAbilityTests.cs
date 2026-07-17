using System;
using NUnit.Framework;
using UnityEngine;
using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using Core;
using Core.Events;
using Ability;
using Ability.Abilities;
using Ability.Buff;
using Ability.Config;
using Ability.Cooldown;
using Ability.Result;
using Tests.Ability;
using Tests.Gameplay.Combat;

namespace Tests.Ability.Abilities
{
    [TestFixture]
    public class DamageAbilityTests
    {
        private RecordingEventBus _targetEventBus;
        private StatCollection _casterStats;
        private StatCollection _targetStats;
        private TestCombatEntity _caster;
        private TestCombatEntity _target;
        private DamagePipeline _pipeline;
        private CooldownManager _cooldownManager;
        private RecordingBuffApplier _buffApplier;
        private DamageAbilityConfig _config;
        private DamageAbility _ability;

        [SetUp]
        public void SetUp()
        {
            _targetEventBus = new RecordingEventBus();
            var casterBus = new RecordingEventBus();
            var targetBus = new RecordingEventBus();
            _casterStats = CreateCasterStats(casterBus);
            _targetStats = CreateTargetStats(targetBus);
            _caster = new TestCombatEntity(_casterStats, casterBus);
            _target = new TestCombatEntity(_targetStats, targetBus);
            _pipeline = new DamagePipeline(Array.Empty<IDamageModifier>());
            _cooldownManager = new CooldownManager(new AbilityRecordingEventBus());
            _buffApplier = new RecordingBuffApplier();
            _config = AbilityConfigTestFactory.CreateDamageConfig(
                damageCoefficient: 1.5f,
                damageType: DamageType.Physical);
            _ability = new DamageAbility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                UnityEngine.Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void Execute_AppliesDamageViaPipeline()
        {
            var context = CreateContext(new FixedRandomProvider(0.99f));

            AbilityExecuteResult result = _ability.Execute(in context);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(1, _targetEventBus.DamageEvents.Count);
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(150f).Within(0.001f));
            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(850f).Within(0.001f));
        }

        [Test]
        public void Execute_UsesRandomProviderForCrit_WhenRollBelowCritChance()
        {
            var context = CreateContext(new FixedRandomProvider(0.4f));

            _ability.Execute(in context);

            Assert.IsTrue(_targetEventBus.DamageEvents[0].IsCrit);
        }

        [Test]
        public void Execute_UsesRandomProviderForCrit_WhenRollAtOrAboveCritChance()
        {
            var context = CreateContext(new FixedRandomProvider(0.5f));

            _ability.Execute(in context);

            Assert.IsFalse(_targetEventBus.DamageEvents[0].IsCrit);
        }

        [Test]
        public void Execute_WithOnHitBuff_AppliesAfterPipeline()
        {
            BuffConfig onHitBuff = AbilityConfigTestFactory.CreateBuffConfig();
            UnityEngine.Object.DestroyImmediate(_config);
            _config = AbilityConfigTestFactory.CreateDamageConfig(onHitBuff: onHitBuff);
            var orderApplier = new OrderRecordingBuffApplier(() => _targetEventBus.DamageEvents.Count > 0);
            var context = CreateContext(new FixedRandomProvider(0.99f), orderApplier);

            try
            {
                _ability.Execute(in context);

                Assert.IsTrue(orderApplier.DamageEventExistedBeforeApply);
                Assert.AreEqual(1, orderApplier.ApplyCalls.Count);
                Assert.AreSame(onHitBuff, orderApplier.ApplyCalls[0].Config);
                Assert.AreSame(_caster, orderApplier.ApplyCalls[0].Instigator);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(onHitBuff);
            }
        }

        [Test]
        public void Execute_WithoutOnHitBuff_DoesNotApply()
        {
            var context = CreateContext(new FixedRandomProvider(0.99f));

            _ability.Execute(in context);

            Assert.IsEmpty(_buffApplier.ApplyCalls);
        }

        [Test]
        public void Execute_ReturnsSuccess()
        {
            var context = CreateContext(new FixedRandomProvider(0.99f));

            AbilityExecuteResult result = _ability.Execute(in context);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(AbilityFailReason.None, result.FailReason);
        }

        [Test]
        public void CanExecute_WhenTargetAlive_ReturnsTrue()
        {
            var context = CreateContext(new FixedRandomProvider(0.99f));

            Assert.IsTrue(_ability.CanExecute(in context));
        }

        [Test]
        public void CanExecute_WhenTargetDead_ReturnsFalse()
        {
            _target.Damageable.TakeDamage(1000f);
            var context = CreateContext(new FixedRandomProvider(0.99f));

            Assert.IsFalse(_ability.CanExecute(in context));
        }

        [Test]
        public void Execute_WhenTargetDead_ReturnsBlockedWithoutDamageOrBuff()
        {
            BuffConfig onHitBuff = AbilityConfigTestFactory.CreateBuffConfig();
            UnityEngine.Object.DestroyImmediate(_config);
            _config = AbilityConfigTestFactory.CreateDamageConfig(onHitBuff: onHitBuff);
            _target.Damageable.TakeDamage(1000f);
            var context = CreateContext(new FixedRandomProvider(0.99f));

            try
            {
                AbilityExecuteResult result = _ability.Execute(in context);

                Assert.IsFalse(result.Succeeded);
                Assert.AreEqual(AbilityFailReason.Blocked, result.FailReason);
                Assert.IsEmpty(_targetEventBus.DamageEvents);
                Assert.IsEmpty(_buffApplier.ApplyCalls);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(onHitBuff);
            }
        }

        [Test]
        public void Execute_PropagatesDamageTypeToDamageEvent()
        {
            UnityEngine.Object.DestroyImmediate(_config);
            _config = AbilityConfigTestFactory.CreateDamageConfig(
                damageCoefficient: 1f,
                damageType: DamageType.Magical);
            var context = CreateContext(new FixedRandomProvider(0.99f));

            _ability.Execute(in context);

            Assert.AreEqual(1, _targetEventBus.DamageEvents.Count);
            Assert.AreEqual(DamageType.Magical, _targetEventBus.DamageEvents[0].DamageType);
        }

        [Test]
        public void Execute_WrongConfigType_ThrowsInvalidOperationException()
        {
            AbilityConfig stubConfig = AbilityConfigTestFactory.CreateStubConfig();
            UnityEngine.Object.DestroyImmediate(_config);
            _config = null;

            var targetDeps = new AbilityTargetDeps(_target, _targetEventBus, _buffApplier);
            var context = new AbilityContext(
                _caster,
                targetDeps,
                abilityIndex: 0,
                stubConfig,
                _pipeline,
                _cooldownManager,
                new FixedRandomProvider(0.99f));

            try
            {
                Assert.Throws<InvalidOperationException>(() => _ability.Execute(in context));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stubConfig);
            }
        }

        private AbilityContext CreateContext(
            IRandomProvider randomProvider,
            IBuffApplier buffApplier = null)
        {
            var targetDeps = new AbilityTargetDeps(
                _target,
                _targetEventBus,
                buffApplier ?? _buffApplier);

            return new AbilityContext(
                _caster,
                targetDeps,
                abilityIndex: 0,
                _config,
                _pipeline,
                _cooldownManager,
                randomProvider);
        }

        private static StatCollection CreateCasterStats(IEventBus bus)
        {
            var stats = new StatCollection(bus);
            stats.SetBaseStat(StatType.MaxHealth, 100f);
            stats.SetBaseStat(StatType.AttackPower, 100f);
            stats.SetBaseStat(StatType.CritChance, 0.5f);
            return stats;
        }

        private static StatCollection CreateTargetStats(IEventBus bus)
        {
            var stats = new StatCollection(bus);
            stats.SetBaseStat(StatType.MaxHealth, 1000f);
            stats.SetBaseStat(StatType.Defense, 0f);
            return stats;
        }
    }
}
