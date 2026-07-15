using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using NUnit.Framework;

namespace Tests.Gameplay.Combat
{
    [TestFixture]
    public class CritModifierTests
    {
        private CritModifier _modifier;

        [SetUp]
        public void SetUp()
        {
            _modifier = new CritModifier();
        }

        [Test]
        public void Modify_WhenNotCrit_ReturnsUnmodifiedDamage()
        {
            var context = CreateContext(critDamage: 2f, isCrit: false);

            float result = _modifier.Modify(50f, context);

            Assert.That(result, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void Modify_WhenCrit_AppliesCritDamageMultiplier()
        {
            var context = CreateContext(critDamage: 2f, isCrit: true);

            float result = _modifier.Modify(50f, context);

            Assert.That(result, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Modify_TrueDamageWithCrit_StillAppliesCritMultiplier()
        {
            var context = CreateContext(critDamage: 1.5f, isCrit: true, DamageType.True);

            float result = _modifier.Modify(40f, context);

            Assert.That(result, Is.EqualTo(60f).Within(0.001f));
        }

        private static DamageContext CreateContext(
            float critDamage,
            bool isCrit,
            DamageType damageType = DamageType.Physical)
        {
            var bus = new NoOpEventBus();
            var sourceStats = new StatCollection(bus);
            sourceStats.SetBaseStat(StatType.MaxHealth, 100f);
            sourceStats.SetBaseStat(StatType.CritDamage, critDamage);

            var targetStats = new StatCollection(bus);
            targetStats.SetBaseStat(StatType.MaxHealth, 100f);

            var source = new TestCombatEntity(sourceStats, bus);
            var target = new TestCombatEntity(targetStats, bus);

            return new DamageContext(source, target, damageType, isCrit);
        }
    }
}
