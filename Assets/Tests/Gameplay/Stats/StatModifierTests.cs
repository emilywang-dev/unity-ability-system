using System;
using NUnit.Framework;
using Gameplay.Stats;

namespace Tests.Gameplay.Stats
{
    [TestFixture]
    public class StatModifierTests
    {
        private static readonly object Source = new();

        [Test]
        public void Constructor_NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, null));
        }

        [Test]
        public void Constructor_StatTypeNone_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StatModifier(StatType.None, StatModifierType.Additive, 10f, Source));
        }

        [Test]
        public void Constructor_NonFiniteValue_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StatModifier(StatType.AttackPower, StatModifierType.Additive, float.NaN, Source));
        }

        [Test]
        public void Constructor_InvalidModifierType_ThrowsArgumentOutOfRangeException()
        {
            var invalidType = (StatModifierType)99;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new StatModifier(StatType.AttackPower, invalidType, 10f, Source));
        }

        [Test]
        public void Constructor_ValidArgs_SetsProperties()
        {
            var modifier = new StatModifier(
                StatType.Defense,
                StatModifierType.Percent,
                0.25f,
                Source);

            Assert.AreEqual(StatType.Defense, modifier.StatType);
            Assert.AreEqual(StatModifierType.Percent, modifier.ModifierType);
            AssertValue(0.25f, modifier.Value);
            Assert.AreSame(Source, modifier.Source);
        }

        private static void AssertValue(float expected, float actual)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(0.001f));
        }
    }
}
