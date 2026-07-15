using System;
using Gameplay.Combat;
using Gameplay.Stats;
using NUnit.Framework;

namespace Tests.Gameplay.Combat
{
    [TestFixture]
    public class DamageContextTests
    {
        private TestCombatEntity _entity;

        [SetUp]
        public void SetUp()
        {
            var bus = new NoOpEventBus();
            var stats = new StatCollection(bus);
            stats.SetBaseStat(StatType.MaxHealth, 100f);
            _entity = new TestCombatEntity(stats, bus);
        }

        [Test]
        public void Constructor_ValidArguments_SetsAllProperties()
        {
            var context = new DamageContext(_entity, _entity, DamageType.Magical, isCrit: true);

            Assert.AreSame(_entity, context.Source);
            Assert.AreSame(_entity, context.Target);
            Assert.AreEqual(DamageType.Magical, context.DamageType);
            Assert.IsTrue(context.IsCrit);
        }

        [Test]
        public void Constructor_NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DamageContext(null, _entity, DamageType.Physical, false));
        }

        [Test]
        public void Constructor_NullTarget_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DamageContext(_entity, null, DamageType.Physical, false));
        }
    }
}
