using System;
using NUnit.Framework;
using Ability.Cooldown;
using Ability.Events;
using Tests.Ability;

namespace Tests.Ability.Cooldown
{
    [TestFixture]
    public class CooldownManagerTests
    {
        private AbilityRecordingEventBus _eventBus;
        private CooldownManager _manager;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new AbilityRecordingEventBus();
            _manager = new CooldownManager(_eventBus);
        }

        [Test]
        public void Constructor_NullEventBus_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CooldownManager(null));
        }

        [Test]
        public void StartCooldown_PublishesCooldownStartedEvent()
        {
            _manager.StartCooldown(1, 3f);

            Assert.AreEqual(1, _eventBus.CooldownStartedEvents.Count);
            Assert.AreEqual(1, _eventBus.CooldownStartedEvents[0].AbilityIndex);
            Assert.That(_eventBus.CooldownStartedEvents[0].Duration, Is.EqualTo(3f).Within(0.001f));
            Assert.IsTrue(_manager.IsOnCooldown(1));
        }

        [Test]
        public void StartCooldown_ZeroDuration_IsNoOp_NoEvent()
        {
            _manager.StartCooldown(0, 0f);

            Assert.IsEmpty(_eventBus.CooldownStartedEvents);
            Assert.IsFalse(_manager.IsOnCooldown(0));
        }

        [Test]
        public void StartCooldown_NegativeDuration_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.StartCooldown(0, -1f));
        }

        [Test]
        public void Tick_WhenRemainingReachesZero_PublishesCooldownEndedEvent()
        {
            _manager.StartCooldown(2, 1.5f);
            _manager.Tick(1.5f);

            Assert.AreEqual(1, _eventBus.CooldownEndedEvents.Count);
            Assert.AreEqual(2, _eventBus.CooldownEndedEvents[0].AbilityIndex);
        }

        [Test]
        public void Tick_WhenRemainingReachesZero_IsOnCooldownFalse()
        {
            _manager.StartCooldown(0, 2f);
            _manager.Tick(2f);

            Assert.IsFalse(_manager.IsOnCooldown(0));
            Assert.That(_manager.GetRemaining(0), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void GetNormalized_AtStart_ReturnsOne()
        {
            _manager.StartCooldown(0, 4f);

            Assert.That(_manager.GetNormalized(0), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void GetNormalized_AtHalf_ReturnsHalf()
        {
            _manager.StartCooldown(0, 4f);
            _manager.Tick(2f);

            Assert.That(_manager.GetNormalized(0), Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void GetNormalized_AfterEnd_ReturnsZero()
        {
            _manager.StartCooldown(0, 2f);
            _manager.Tick(2f);

            Assert.That(_manager.GetNormalized(0), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void GetNormalized_NoCooldown_ReturnsZero()
        {
            Assert.That(_manager.GetNormalized(0), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void GetRemaining_NoCooldown_ReturnsZero()
        {
            Assert.That(_manager.GetRemaining(0), Is.EqualTo(0f).Within(0.001f));
            Assert.IsFalse(_manager.IsOnCooldown(0));
        }

        [Test]
        public void MultipleSlots_CooldownsAreIndependent()
        {
            _manager.StartCooldown(0, 4f);
            _manager.StartCooldown(2, 1f);

            _manager.Tick(1f);

            Assert.IsTrue(_manager.IsOnCooldown(0));
            Assert.That(_manager.GetRemaining(0), Is.EqualTo(3f).Within(0.001f));
            Assert.IsFalse(_manager.IsOnCooldown(2));
            Assert.That(_manager.GetRemaining(2), Is.EqualTo(0f).Within(0.001f));
            Assert.AreEqual(1, _eventBus.CooldownEndedEvents.Count);
            Assert.AreEqual(2, _eventBus.CooldownEndedEvents[0].AbilityIndex);
        }

        [Test]
        public void StartCooldown_OnSameIndex_OverwritesRemaining()
        {
            _manager.StartCooldown(0, 4f);
            _manager.StartCooldown(0, 1f);

            Assert.That(_manager.GetRemaining(0), Is.EqualTo(1f).Within(0.001f));
            Assert.That(_manager.GetNormalized(0), Is.EqualTo(1f).Within(0.001f));
            Assert.AreEqual(2, _eventBus.CooldownStartedEvents.Count);
        }
    }
}
