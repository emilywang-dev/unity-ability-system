using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using Ability;
using Ability.Abilities;
using Ability.Buff;
using Ability.Config;
using Ability.Result;
using Tests.Ability;
using Tests.Gameplay.Combat;

namespace Tests.Ability
{
    [TestFixture]
    public class AbilitySystemTests
    {
        private AbilityRecordingEventBus _localEventBus;
        private RecordingEventBus _targetEventBus;
        private StatCollection _casterStats;
        private StatCollection _targetStats;
        private TestCombatEntity _caster;
        private TestCombatEntity _target;
        private DamagePipeline _pipeline;
        private FixedRandomProvider _randomProvider;
        private RecordingBuffApplier _buffApplier;
        private MockAbility _mockAbility;
        private FixedAbilityFactory _mockAbilityFactory;
        private AbilityConfig _config;
        private readonly List<ScriptableObject> _createdAssets = new();

        [SetUp]
        public void SetUp()
        {
            _localEventBus = new AbilityRecordingEventBus();
            _targetEventBus = new RecordingEventBus();
            var casterBus = new RecordingEventBus();
            var targetBus = new RecordingEventBus();
            _casterStats = CreateStats(casterBus);
            _targetStats = CreateStats(targetBus);
            _caster = new TestCombatEntity(_casterStats, casterBus);
            _target = new TestCombatEntity(_targetStats, targetBus);
            _pipeline = new DamagePipeline(Array.Empty<IDamageModifier>());
            _randomProvider = new FixedRandomProvider(0.5f);
            _buffApplier = new RecordingBuffApplier();
            _mockAbility = new MockAbility();
            _mockAbilityFactory = new FixedAbilityFactory(_mockAbility);
            _config = Track(AbilityConfigTestFactory.CreateDamageConfig(cooldownDuration: 2f));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (ScriptableObject asset in _createdAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            _createdAssets.Clear();
        }

        [Test]
        public void Constructor_NullSlots_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilitySystem(null, _localEventBus, _randomProvider, _pipeline, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_NullLocalEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilitySystem(CreateSlots(), null, _randomProvider, _pipeline, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_NullRandomProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilitySystem(CreateSlots(), _localEventBus, null, _pipeline, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_NullDamagePipeline_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilitySystem(CreateSlots(), _localEventBus, _randomProvider, null, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_NullAbilityFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilitySystem(CreateSlots(), _localEventBus, _randomProvider, _pipeline, null));
        }

        [Test]
        public void Constructor_MissingSlotConfig_ThrowsInvalidOperationException()
        {
            var slots = new List<AbilitySlot>
            {
                new AbilitySlot { Config = null, AbilityType = AbilityType.Damage }
            };

            Assert.Throws<InvalidOperationException>(() =>
                new AbilitySystem(slots, _localEventBus, _randomProvider, _pipeline, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_MissingSlotAbilityType_ThrowsInvalidOperationException()
        {
            var slots = new List<AbilitySlot>
            {
                new AbilitySlot { Config = _config, AbilityType = AbilityType.None }
            };

            Assert.Throws<InvalidOperationException>(() =>
                new AbilitySystem(slots, _localEventBus, _randomProvider, _pipeline, _mockAbilityFactory));
        }

        [Test]
        public void Constructor_MismatchedConfigType_ThrowsInvalidOperationException()
        {
            AbilityConfig stubConfig = Track(AbilityConfigTestFactory.CreateStubConfig());
            var slots = new List<AbilitySlot>
            {
                new AbilitySlot { Config = stubConfig, AbilityType = AbilityType.Damage }
            };

            Assert.Throws<InvalidOperationException>(() =>
                new AbilitySystem(slots, _localEventBus, _randomProvider, _pipeline, new AbilityFactory()));
        }

        [Test]
        public void TryExecute_InvalidAbilityIndex_ThrowsArgumentOutOfRangeException()
        {
            var system = CreateSystem();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                system.TryExecute(1, _caster, CreateTargetDeps()));
        }

        [Test]
        public void TryExecute_NegativeAbilityIndex_ThrowsArgumentOutOfRangeException()
        {
            var system = CreateSystem();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                system.TryExecute(-1, _caster, CreateTargetDeps()));
        }

        [Test]
        public void TryExecute_WithAbilityFactory_DamageAbility_ReturnsSuccessAppliesDamageCooldownAndBuff()
        {
            BuffConfig onHitBuff = Track(AbilityConfigTestFactory.CreateBuffConfig());
            UnityEngine.Object.DestroyImmediate(_config);
            _config = Track(AbilityConfigTestFactory.CreateDamageConfig(
                cooldownDuration: 2f,
                damageCoefficient: 1f,
                onHitBuff: onHitBuff));
            // Target still has CurrentHealth 100 from SetUp; keep damage below max so health stays > 0.
            _casterStats.SetBaseStat(StatType.AttackPower, 40f);
            _casterStats.SetBaseStat(StatType.CritChance, 0f);
            var slots = new List<AbilitySlot>
            {
                new AbilitySlot { Config = _config, AbilityType = AbilityType.Damage }
            };
            var system = new AbilitySystem(
                slots,
                _localEventBus,
                _randomProvider,
                _pipeline,
                new AbilityFactory());

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(system.CooldownQuery.IsOnCooldown(0));
            Assert.AreEqual(1, _localEventBus.CooldownStartedEvents.Count);
            Assert.AreEqual(1, _targetEventBus.DamageEvents.Count);
            Assert.That(_targetEventBus.DamageEvents[0].Amount, Is.EqualTo(40f).Within(0.001f));
            Assert.That(_target.Damageable.CurrentHealth, Is.EqualTo(60f).Within(0.001f));
            Assert.AreEqual(1, _buffApplier.ApplyCalls.Count);
            Assert.AreSame(onHitBuff, _buffApplier.ApplyCalls[0].Config);
            Assert.AreSame(_caster, _buffApplier.ApplyCalls[0].Instigator);
        }

        [Test]
        public void TryExecute_DeadTarget_WithDamageAbility_ReturnsBlockedWithoutCooldownOrBuff()
        {
            BuffConfig onHitBuff = Track(AbilityConfigTestFactory.CreateBuffConfig());
            UnityEngine.Object.DestroyImmediate(_config);
            _config = Track(AbilityConfigTestFactory.CreateDamageConfig(
                cooldownDuration: 2f,
                onHitBuff: onHitBuff));
            var slots = new List<AbilitySlot>
            {
                new AbilitySlot { Config = _config, AbilityType = AbilityType.Damage }
            };
            var system = new AbilitySystem(
                slots,
                _localEventBus,
                _randomProvider,
                _pipeline,
                new AbilityFactory());
            _target.Damageable.TakeDamage(100f);

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsTrue(result.Failed);
            Assert.AreEqual(AbilityFailReason.Blocked, result.FailReason);
            Assert.IsFalse(system.CooldownQuery.IsOnCooldown(0));
            Assert.IsEmpty(_localEventBus.CooldownStartedEvents);
            Assert.IsEmpty(_buffApplier.ApplyCalls);
            Assert.IsEmpty(_targetEventBus.DamageEvents);
        }

        [Test]
        public void TryExecute_NullCaster_ThrowsArgumentNullException()
        {
            var system = CreateSystem();

            Assert.Throws<ArgumentNullException>(() =>
                system.TryExecute(0, null, CreateTargetDeps()));
        }

        [Test]
        public void TryExecute_NullCaster_WhileOnCooldown_StillThrowsArgumentNullException()
        {
            var system = CreateSystem();
            system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.Throws<ArgumentNullException>(() =>
                system.TryExecute(0, null, CreateTargetDeps()));
        }

        [Test]
        public void TryExecute_InvalidTargetDeps_ThrowsArgumentException()
        {
            var system = CreateSystem();

            Assert.Throws<ArgumentException>(() =>
                system.TryExecute(0, _caster, default));
        }

        [Test]
        public void TryExecute_InvalidTargetDeps_WhileOnCooldown_StillThrowsArgumentException()
        {
            var system = CreateSystem();
            system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.Throws<ArgumentException>(() =>
                system.TryExecute(0, _caster, default));
        }

        [Test]
        public void TryExecute_OnCooldown_ReturnsOnCooldownWithoutCallingCanExecuteOrExecute()
        {
            var system = CreateSystem();
            system.TryExecute(0, _caster, CreateTargetDeps());
            _mockAbility.CanExecuteCallCount = 0;
            _mockAbility.ExecuteCallCount = 0;

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.IsOnCooldown);
            Assert.IsFalse(result.Failed);
            Assert.AreEqual(0, _mockAbility.CanExecuteCallCount);
            Assert.AreEqual(0, _mockAbility.ExecuteCallCount);
        }

        [Test]
        public void TryExecute_CanExecuteFalse_ReturnsBlockedWithoutCallingExecute()
        {
            _mockAbility.CanExecuteResult = false;
            var system = CreateSystem();

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsFalse(result.Succeeded);
            Assert.IsFalse(result.IsOnCooldown);
            Assert.IsTrue(result.Failed);
            Assert.AreEqual(AbilityFailReason.Blocked, result.FailReason);
            Assert.AreEqual(1, _mockAbility.CanExecuteCallCount);
            Assert.AreEqual(0, _mockAbility.ExecuteCallCount);
            Assert.IsFalse(system.CooldownQuery.IsOnCooldown(0));
        }

        [Test]
        public void TryExecute_ExecuteSuccess_StartsCooldown()
        {
            var system = CreateSystem();

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(system.CooldownQuery.IsOnCooldown(0));
            Assert.AreEqual(1, _localEventBus.CooldownStartedEvents.Count);
            Assert.AreEqual(0, _localEventBus.CooldownStartedEvents[0].AbilityIndex);
            Assert.That(_localEventBus.CooldownStartedEvents[0].Duration, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void TryExecute_ExecuteSuccess_WithZeroCooldownDuration_DoesNotStartCooldown()
        {
            UnityEngine.Object.DestroyImmediate(_config);
            _config = Track(AbilityConfigTestFactory.CreateDamageConfig(cooldownDuration: 0f));
            var system = CreateSystem();

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.IsOnCooldown);
            Assert.IsFalse(system.CooldownQuery.IsOnCooldown(0));
            Assert.IsEmpty(_localEventBus.CooldownStartedEvents);
        }

        [Test]
        public void TryExecute_ExecuteFailure_DoesNotStartCooldown()
        {
            _mockAbility.ExecuteResult = AbilityExecuteResult.Fail(AbilityFailReason.Interrupted);
            var system = CreateSystem();

            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Failed);
            Assert.AreEqual(AbilityFailReason.Interrupted, result.FailReason);
            Assert.IsFalse(system.CooldownQuery.IsOnCooldown(0));
            Assert.IsEmpty(_localEventBus.CooldownStartedEvents);
        }

        [Test]
        public void TryExecute_DoesNotApplyOnHitBuff()
        {
            var system = CreateSystem();

            system.TryExecute(0, _caster, CreateTargetDeps());

            Assert.IsEmpty(_buffApplier.ApplyCalls);
        }

        [Test]
        public void Tick_ForwardsToCooldownManager_AllowsExecuteAfterCooldownEnds()
        {
            var system = CreateSystem();
            system.TryExecute(0, _caster, CreateTargetDeps());
            _mockAbility.CanExecuteCallCount = 0;
            _mockAbility.ExecuteCallCount = 0;

            system.Tick(2f);

            Assert.IsFalse(system.CooldownQuery.IsOnCooldown(0));
            Assert.AreEqual(1, _localEventBus.CooldownEndedEvents.Count);
            TryExecuteResult result = system.TryExecute(0, _caster, CreateTargetDeps());
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(1, _mockAbility.CanExecuteCallCount);
            Assert.AreEqual(1, _mockAbility.ExecuteCallCount);
        }

        private AbilitySystem CreateSystem()
        {
            return new AbilitySystem(
                CreateSlots(),
                _localEventBus,
                _randomProvider,
                _pipeline,
                _mockAbilityFactory);
        }

        private List<AbilitySlot> CreateSlots()
        {
            return new List<AbilitySlot>
            {
                // AbilityType is required by ctor; FixedAbilityFactory ignores it and returns MockAbility.
                new AbilitySlot { Config = _config, AbilityType = AbilityType.Damage }
            };
        }

        private AbilityTargetDeps CreateTargetDeps()
        {
            return new AbilityTargetDeps(_target, _targetEventBus, _buffApplier);
        }

        private T Track<T>(T asset) where T : ScriptableObject
        {
            _createdAssets.Add(asset);
            return asset;
        }

        private static StatCollection CreateStats(IEventBus bus)
        {
            var stats = new StatCollection(bus);
            stats.SetBaseStat(StatType.MaxHealth, 100f);
            return stats;
        }
    }
}
