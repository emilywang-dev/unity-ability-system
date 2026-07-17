using System;
using NUnit.Framework;
using Core;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using Ability;
using Ability.Config;
using Ability.Cooldown;
using Tests.Ability;
using Tests.Gameplay.Combat;

namespace Tests.Ability
{
    [TestFixture]
    public class AbilityContextTests
    {
        private RecordingEventBus _bus;
        private TestCombatEntity _caster;
        private TestCombatEntity _target;
        private AbilityTargetDeps _validTarget;
        private DamageAbilityConfig _config;
        private DamagePipeline _pipeline;
        private CooldownManager _cooldowns;
        private FixedRandomProvider _randomProvider;
        private RecordingBuffApplier _buffApplier;

        [SetUp]
        public void SetUp()
        {
            _bus = new RecordingEventBus();
            var stats = new StatCollection(_bus);
            stats.SetBaseStat(StatType.MaxHealth, 100f);
            _caster = new TestCombatEntity(stats, _bus);
            var targetBus = new RecordingEventBus();
            var targetStats = new StatCollection(targetBus);
            targetStats.SetBaseStat(StatType.MaxHealth, 100f);
            _target = new TestCombatEntity(targetStats, targetBus);
            _buffApplier = new RecordingBuffApplier();
            _validTarget = new AbilityTargetDeps(_target, targetBus, _buffApplier);
            _config = AbilityConfigTestFactory.CreateDamageConfig();
            _pipeline = new DamagePipeline(Array.Empty<IDamageModifier>());
            _cooldowns = new CooldownManager(new AbilityRecordingEventBus());
            _randomProvider = new FixedRandomProvider(0.5f);
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
        public void Constructor_NullCaster_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityContext(null, _validTarget, 0, _config, _pipeline, _cooldowns, _randomProvider));
        }

        [Test]
        public void Constructor_InvalidTarget_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new AbilityContext(_caster, default, 0, _config, _pipeline, _cooldowns, _randomProvider));
        }

        [Test]
        public void Constructor_NegativeAbilityIndex_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new AbilityContext(_caster, _validTarget, -1, _config, _pipeline, _cooldowns, _randomProvider));
        }

        [Test]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityContext(_caster, _validTarget, 0, null, _pipeline, _cooldowns, _randomProvider));
        }

        [Test]
        public void Constructor_NullDamagePipeline_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityContext(_caster, _validTarget, 0, _config, null, _cooldowns, _randomProvider));
        }

        [Test]
        public void Constructor_NullCooldownQuery_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityContext(_caster, _validTarget, 0, _config, _pipeline, null, _randomProvider));
        }

        [Test]
        public void Constructor_NullRandomProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityContext(_caster, _validTarget, 0, _config, _pipeline, _cooldowns, null));
        }

        [Test]
        public void Constructor_ValidArgs_ExposesTargetProxies()
        {
            var context = new AbilityContext(
                _caster, _validTarget, 0, _config, _pipeline, _cooldowns, _randomProvider);

            Assert.AreSame(_target, context.TargetEntity);
            Assert.AreSame(_validTarget.EventBus, context.TargetEventBus);
            Assert.AreSame(_buffApplier, context.TargetBuffApplier);
        }
    }

    [TestFixture]
    public class AbilityTargetDepsTests
    {
        private RecordingEventBus _bus;
        private TestCombatEntity _entity;
        private RecordingBuffApplier _buffApplier;

        [SetUp]
        public void SetUp()
        {
            _bus = new RecordingEventBus();
            var stats = new StatCollection(_bus);
            stats.SetBaseStat(StatType.MaxHealth, 100f);
            _entity = new TestCombatEntity(stats, _bus);
            _buffApplier = new RecordingBuffApplier();
        }

        [Test]
        public void Constructor_NullEntity_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityTargetDeps(null, _bus, _buffApplier));
        }

        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityTargetDeps(_entity, null, _buffApplier));
        }

        [Test]
        public void Constructor_NullBuffApplier_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AbilityTargetDeps(_entity, _bus, null));
        }

        [Test]
        public void IsValid_Default_IsFalse()
        {
            Assert.IsFalse(default(AbilityTargetDeps).IsValid);
        }

        [Test]
        public void IsValid_Constructed_IsTrue()
        {
            var deps = new AbilityTargetDeps(_entity, _bus, _buffApplier);

            Assert.IsTrue(deps.IsValid);
        }
    }
}
