using System;
using NUnit.Framework;
using Gameplay.Stats;

namespace Tests.Gameplay.Stats
{
    [TestFixture]
    public class StatTests
    {
        private static readonly object SourceA = new();
        private static readonly object SourceB = new();

        [Test]
        public void Constructor_NonFiniteBase_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Stat(float.PositiveInfinity));
        }

        [Test]
        public void Constructor_ValidBase_GetRawValueEqualsBase()
        {
            var stat = new Stat(100f);

            AssertValue(100f, stat.GetRawValue());
        }

        [Test]
        public void SetBaseValue_UpdatesValueAndInvalidatesCache()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));
            AssertValue(110f, stat.GetRawValue());

            stat.SetBaseValue(200f);

            AssertValue(210f, stat.GetRawValue());
        }

        [Test]
        public void SetBaseValue_NonFinite_ThrowsArgumentOutOfRangeException()
        {
            var stat = new Stat(100f);

            Assert.Throws<ArgumentOutOfRangeException>(() => stat.SetBaseValue(float.NaN));
        }

        [Test]
        public void AddModifier_Null_ThrowsArgumentNullException()
        {
            var stat = new Stat(100f);

            Assert.Throws<ArgumentNullException>(() => stat.AddModifier(null));
        }

        [Test]
        public void AddModifier_Additive_AppliesToBase()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA));

            AssertValue(125f, stat.GetRawValue());
        }

        [Test]
        public void AddModifier_PercentModifier_AppliesPercentageBonus()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Percent, 0.1f, SourceA));

            AssertValue(110f, stat.GetRawValue());
        }

        [Test]
        public void GetRawValue_MixedAdditiveAndPercent_UsesDocumentedFormula()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 50f, SourceA));
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Percent, 0.1f, SourceA));
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Percent, 0.2f, SourceA));

            AssertValue(195f, stat.GetRawValue());
        }

        [Test]
        public void RemoveModifier_Existing_ReturnsTrueAndRestoresValue()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA);
            stat.AddModifier(modifier);

            var removed = stat.RemoveModifier(modifier);

            Assert.IsTrue(removed);
            AssertValue(100f, stat.GetRawValue());
        }

        [Test]
        public void RemoveModifier_Absent_ReturnsFalse()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA);

            Assert.IsFalse(stat.RemoveModifier(modifier));
        }

        [Test]
        public void RemoveModifier_Null_ThrowsArgumentNullException()
        {
            var stat = new Stat(100f);

            Assert.Throws<ArgumentNullException>(() => stat.RemoveModifier(null));
        }

        [Test]
        public void RemoveModifiersFromSource_RemovesOnlyMatchingSource()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 20f, SourceB));

            var removed = stat.RemoveModifiersFromSource(SourceA);

            Assert.IsTrue(removed);
            AssertValue(120f, stat.GetRawValue());
        }

        [Test]
        public void RemoveModifiersFromSource_NoMatch_ReturnsFalse()
        {
            var stat = new Stat(100f);
            stat.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));

            Assert.IsFalse(stat.RemoveModifiersFromSource(SourceB));
        }

        [Test]
        public void GetRawValue_AfterAddThenRemove_CacheReflectsCurrentModifiers()
        {
            var stat = new Stat(100f);
            var modifier = new StatModifier(StatType.AttackPower, StatModifierType.Additive, 50f, SourceA);

            stat.AddModifier(modifier);
            AssertValue(150f, stat.GetRawValue());

            stat.RemoveModifier(modifier);
            AssertValue(100f, stat.GetRawValue());
        }

        private static void AssertValue(float expected, float actual)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(0.001f));
        }
    }
}
