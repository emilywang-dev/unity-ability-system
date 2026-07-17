using System;
using System.Collections.Generic;
using NUnit.Framework;
using Core.Events;
using Gameplay.Stats;
using Gameplay.Stats.Events;

namespace Tests.Gameplay.Stats
{
    [TestFixture]
    public class StatCollectionTests
    {
        private static readonly object SourceA = new();
        private static readonly object SourceB = new();

        private RecordingEventBus _eventBus;
        private StatCollection _collection;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new RecordingEventBus();
            _collection = new StatCollection(_eventBus);
        }

        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new StatCollection(null));
        }

        [Test]
        public void SetBaseStat_StatTypeNone_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _collection.SetBaseStat(StatType.None, 100f));
        }

        [Test]
        public void SetBaseStat_NonFinite_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _collection.SetBaseStat(StatType.MaxHealth, float.NaN));
        }

        [Test]
        public void SetBaseStat_FirstCall_GetValueReturnsBase()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);

            AssertValue(100f, _collection.GetValue(StatType.MaxHealth));
        }

        [Test]
        public void SetBaseStat_FirstCall_PublishesStatChangedWithOldEqualsNew()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);

            Assert.AreEqual(1, _eventBus.Published.Count);
            var evt = _eventBus.Published[0];
            Assert.AreEqual(StatType.MaxHealth, evt.StatType);
            AssertValue(100f, evt.OldValue);
            AssertValue(100f, evt.NewValue);
        }

        [Test]
        public void SetBaseStat_Existing_UpdatesValueAndPublishesWhenChanged()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);
            _eventBus.Published.Clear();

            _collection.SetBaseStat(StatType.MaxHealth, 150f);

            AssertValue(150f, _collection.GetValue(StatType.MaxHealth));
            Assert.AreEqual(1, _eventBus.Published.Count);
            AssertValue(100f, _eventBus.Published[0].OldValue);
            AssertValue(150f, _eventBus.Published[0].NewValue);
        }

        [Test]
        public void SetBaseStat_WhenResolvedValueUnchanged_DoesNotPublish()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);
            _eventBus.Published.Clear();

            _collection.SetBaseStat(StatType.MaxHealth, 100f);

            Assert.AreEqual(0, _eventBus.Published.Count);
        }

        [Test]
        public void SetBaseStat_WithExistingModifier_PreservesModifierContribution()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 20f, SourceA));
            _eventBus.Published.Clear();

            _collection.SetBaseStat(StatType.AttackPower, 200f);

            AssertValue(220f, _collection.GetValue(StatType.AttackPower));
            Assert.AreEqual(1, _eventBus.Published.Count);
            AssertValue(120f, _eventBus.Published[0].OldValue);
            AssertValue(220f, _eventBus.Published[0].NewValue);
        }

        [Test]
        public void GetValue_Uninitialized_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => _collection.GetValue(StatType.MaxHealth));
        }

        [Test]
        public void TryGetValue_Uninitialized_ReturnsFalse()
        {
            var found = _collection.TryGetValue(StatType.MaxHealth, out var value);

            Assert.IsFalse(found);
            AssertValue(0f, value);
        }

        [Test]
        public void TryGetValue_Initialized_ReturnsTrueAndValue()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);

            var found = _collection.TryGetValue(StatType.MaxHealth, out var value);

            Assert.IsTrue(found);
            AssertValue(100f, value);
        }

        [Test]
        public void AddModifier_Null_ThrowsArgumentNullException()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 100f);

            Assert.Throws<ArgumentNullException>(() => _collection.AddModifier(null));
        }

        [Test]
        public void AddModifier_UninitializedStat_ThrowsInvalidOperationException()
        {
            var modifier = new StatModifier(StatType.MaxHealth, StatModifierType.Additive, 10f, SourceA);

            Assert.Throws<InvalidOperationException>(() => _collection.AddModifier(modifier));
        }

        [Test]
        public void AddModifier_UpdatesGetValue()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA));

            AssertValue(125f, _collection.GetValue(StatType.AttackPower));
        }

        [Test]
        public void AddModifier_PercentModifier_UpdatesGetValue()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Percent, 0.25f, SourceA));

            AssertValue(125f, _collection.GetValue(StatType.AttackPower));
        }

        [Test]
        public void RemoveModifier_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _collection.RemoveModifier(null));
        }

        [Test]
        public void RemoveModifier_UninitializedStat_ReturnsFalse()
        {
            var modifier = new StatModifier(StatType.MaxHealth, StatModifierType.Additive, 10f, SourceA);

            Assert.IsFalse(_collection.RemoveModifier(modifier));
        }

        [Test]
        public void RemoveModifier_Existing_ReturnsTrueAndRestoresValue()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            var modifier = new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA);
            _collection.AddModifier(modifier);

            var removed = _collection.RemoveModifier(modifier);

            Assert.IsTrue(removed);
            AssertValue(100f, _collection.GetValue(StatType.AttackPower));
        }

        [Test]
        public void RemoveModifiersFromSource_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _collection.RemoveModifiersFromSource(null));
        }

        [Test]
        public void RemoveModifiersFromSource_RemovesAcrossMultipleStats()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.SetBaseStat(StatType.Defense, 50f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));
            _collection.AddModifier(new StatModifier(StatType.Defense, StatModifierType.Additive, 5f, SourceA));
            _eventBus.Published.Clear();

            _collection.RemoveModifiersFromSource(SourceA);

            AssertValue(100f, _collection.GetValue(StatType.AttackPower));
            AssertValue(50f, _collection.GetValue(StatType.Defense));
            Assert.AreEqual(2, _eventBus.Published.Count);
        }

        [Test]
        public void RemoveModifiersFromSource_OnlyMatchingSource()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 20f, SourceB));
            _eventBus.Published.Clear();

            _collection.RemoveModifiersFromSource(SourceA);

            AssertValue(120f, _collection.GetValue(StatType.AttackPower));
            Assert.AreEqual(1, _eventBus.Published.Count);
        }

        [Test]
        public void GetValue_MaxHealth_ClampsToNonNegative()
        {
            _collection.SetBaseStat(StatType.MaxHealth, 10f);
            _collection.AddModifier(new StatModifier(StatType.MaxHealth, StatModifierType.Additive, -50f, SourceA));

            AssertValue(0f, _collection.GetValue(StatType.MaxHealth));
        }

        [Test]
        public void GetValue_MoveSpeed_ClampsToNonNegative()
        {
            _collection.SetBaseStat(StatType.MoveSpeed, 5f);
            _collection.AddModifier(new StatModifier(StatType.MoveSpeed, StatModifierType.Additive, -10f, SourceA));

            AssertValue(0f, _collection.GetValue(StatType.MoveSpeed));
        }

        [Test]
        public void GetValue_CritChance_ClampsToZeroOne()
        {
            _collection.SetBaseStat(StatType.CritChance, 0.5f);
            _collection.AddModifier(new StatModifier(StatType.CritChance, StatModifierType.Percent, 2f, SourceA));

            AssertValue(1f, _collection.GetValue(StatType.CritChance));
        }

        [Test]
        public void GetValue_AttackPower_AllowsNegative()
        {
            _collection.SetBaseStat(StatType.AttackPower, 10f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, -20f, SourceA));

            AssertValue(-10f, _collection.GetValue(StatType.AttackPower));
        }

        [Test]
        public void AddModifier_WhenClampMakesValueUnchanged_DoesNotPublish()
        {
            _collection.SetBaseStat(StatType.CritChance, 1f);
            _eventBus.Published.Clear();

            _collection.AddModifier(new StatModifier(StatType.CritChance, StatModifierType.Percent, 0.5f, SourceA));

            AssertValue(1f, _collection.GetValue(StatType.CritChance));
            Assert.AreEqual(0, _eventBus.Published.Count);
        }

        [Test]
        public void AddModifier_PublishesStatChangedWithOldAndNew()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _eventBus.Published.Clear();

            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA));

            Assert.AreEqual(1, _eventBus.Published.Count);
            var evt = _eventBus.Published[0];
            Assert.AreEqual(StatType.AttackPower, evt.StatType);
            AssertValue(100f, evt.OldValue);
            AssertValue(125f, evt.NewValue);
        }

        [Test]
        public void RemoveModifier_PublishesStatChangedWhenValueChanges()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            var modifier = new StatModifier(StatType.AttackPower, StatModifierType.Additive, 25f, SourceA);
            _collection.AddModifier(modifier);
            _eventBus.Published.Clear();

            _collection.RemoveModifier(modifier);

            Assert.AreEqual(1, _eventBus.Published.Count);
            AssertValue(125f, _eventBus.Published[0].OldValue);
            AssertValue(100f, _eventBus.Published[0].NewValue);
        }

        [Test]
        public void RemoveModifier_WhenClampChangesValue_Publishes()
        {
            _collection.SetBaseStat(StatType.CritChance, 0.5f);
            var modifier = new StatModifier(StatType.CritChance, StatModifierType.Percent, 1f, SourceA);
            _collection.AddModifier(modifier);
            AssertValue(1f, _collection.GetValue(StatType.CritChance));
            _eventBus.Published.Clear();

            _collection.RemoveModifier(modifier);

            AssertValue(0.5f, _collection.GetValue(StatType.CritChance));
            Assert.AreEqual(1, _eventBus.Published.Count);
            AssertValue(1f, _eventBus.Published[0].OldValue);
            AssertValue(0.5f, _eventBus.Published[0].NewValue);
        }

        [Test]
        public void RemoveModifiersFromSource_PublishesForEachChangedStat()
        {
            _collection.SetBaseStat(StatType.AttackPower, 100f);
            _collection.SetBaseStat(StatType.Defense, 50f);
            _collection.AddModifier(new StatModifier(StatType.AttackPower, StatModifierType.Additive, 10f, SourceA));
            _collection.AddModifier(new StatModifier(StatType.Defense, StatModifierType.Additive, 5f, SourceA));
            _eventBus.Published.Clear();

            _collection.RemoveModifiersFromSource(SourceA);

            Assert.AreEqual(2, _eventBus.Published.Count);
        }

        private static void AssertValue(float expected, float actual)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(0.001f));
        }

        private sealed class RecordingEventBus : IEventBus
        {
            public List<StatChangedEvent> Published { get; } = new();

            public void Subscribe<T>(Action<T> handler) where T : IEvent
            {
                throw new NotSupportedException("RecordingEventBus only records Publish calls.");
            }

            public void Unsubscribe<T>(Action<T> handler) where T : IEvent
            {
                throw new NotSupportedException("RecordingEventBus only records Publish calls.");
            }

            public void Publish<T>(T evt) where T : IEvent
            {
                if (evt is StatChangedEvent statChanged)
                {
                    Published.Add(statChanged);
                }
            }
        }
    }
}
