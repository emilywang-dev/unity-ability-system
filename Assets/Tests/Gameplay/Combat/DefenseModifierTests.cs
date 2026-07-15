using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Gameplay.Stats;
using NUnit.Framework;

namespace Tests.Gameplay.Combat
{
    [TestFixture]
    public class DefenseModifierTests
    {
        private DefenseModifier _modifier;

        [SetUp]
        public void SetUp()
        {
            _modifier = new DefenseModifier();
        }

        [Test]
        public void Modify_AppliesMultiplicativeFormula()
        {
            var context = CreateContext(defense: 100f, DamageType.Physical);

            float result = _modifier.Modify(100f, context);

            Assert.That(result, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void Modify_TrueDamage_SkipsDefense()
        {
            var context = CreateContext(defense: 100f, DamageType.True);

            float result = _modifier.Modify(100f, context);

            Assert.That(result, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Modify_DefenseAtNegativeConstant_ReturnsUnmodifiedDamage()
        {
            var context = CreateContext(defense: -100f, DamageType.Physical);

            float result = _modifier.Modify(100f, context);

            Assert.That(result, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Modify_DefenseBelowNegativeConstant_ReturnsUnmodifiedDamage()
        {
            var context = CreateContext(defense: -150f, DamageType.Physical);

            float result = _modifier.Modify(100f, context);

            Assert.That(result, Is.EqualTo(100f).Within(0.001f));
        }

        private static DamageContext CreateContext(float defense, DamageType damageType)
        {
            var bus = new NoOpEventBus();
            var sourceStats = new StatCollection(bus);
            sourceStats.SetBaseStat(StatType.MaxHealth, 100f);

            var targetStats = new StatCollection(bus);
            targetStats.SetBaseStat(StatType.MaxHealth, 100f);
            targetStats.SetBaseStat(StatType.Defense, defense);

            var source = new TestCombatEntity(sourceStats, bus);
            var target = new TestCombatEntity(targetStats, bus);

            return new DamageContext(source, target, damageType, isCrit: false);
        }
    }
}
