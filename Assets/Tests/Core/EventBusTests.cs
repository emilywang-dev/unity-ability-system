using System;
using Core.Events;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public class EventBusTests
    {
        private EventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
        }

        [Test]
        public void Subscribe_NullHandler_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe<TestEvent>(null));
        }

        [Test]
        public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Unsubscribe<TestEvent>(null));
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(1)));
        }

        [Test]
        public void Publish_WithSubscriber_InvokesHandlerWithSameEvent()
        {
            TestEvent received = default;
            var evt = new TestEvent(42);

            _bus.Subscribe<TestEvent>(e => received = e);
            _bus.Publish(evt);

            Assert.AreEqual(42, received.Value);
        }

        [Test]
        public void Publish_WithTwoSubscribers_InvokesBoth()
        {
            var firstCalled = false;
            var secondCalled = false;

            _bus.Subscribe<TestEvent>(_ => firstCalled = true);
            _bus.Subscribe<TestEvent>(_ => secondCalled = true);
            _bus.Publish(new TestEvent(1));

            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void Unsubscribe_OneOfTwo_OtherStillReceives()
        {
            var firstCalled = false;
            var secondCalled = false;
            Action<TestEvent> first = _ => firstCalled = true;
            Action<TestEvent> second = _ => secondCalled = true;

            _bus.Subscribe(first);
            _bus.Subscribe(second);
            _bus.Unsubscribe(first);
            _bus.Publish(new TestEvent(1));

            Assert.IsFalse(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void Unsubscribe_LastSubscriber_PublishDoesNotInvoke()
        {
            var called = false;
            Action<TestEvent> handler = _ => called = true;

            _bus.Subscribe(handler);
            _bus.Unsubscribe(handler);
            _bus.Publish(new TestEvent(1));

            Assert.IsFalse(called);
        }

        [Test]
        public void Unsubscribe_NeverSubscribed_NoOp()
        {
            Action<TestEvent> handler = _ => { };

            Assert.DoesNotThrow(() => _bus.Unsubscribe(handler));
        }

        [Test]
        public void Publish_DifferentEventTypes_AreIsolated()
        {
            var testEventCalled = false;
            var otherEventCalled = false;

            _bus.Subscribe<TestEvent>(_ => testEventCalled = true);
            _bus.Subscribe<OtherTestEvent>(_ => otherEventCalled = true);
            _bus.Publish(new TestEvent(1));

            Assert.IsTrue(testEventCalled);
            Assert.IsFalse(otherEventCalled);
        }

        [Test]
        public void Publish_WhenFirstHandlerThrows_SecondIsNotInvoked()
        {
            var secondCalled = false;

            _bus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("fail"));
            _bus.Subscribe<TestEvent>(_ => secondCalled = true);

            Assert.Throws<InvalidOperationException>(() => _bus.Publish(new TestEvent(1)));
            Assert.IsFalse(secondCalled);
        }

        [Test]
        public void Subscribe_SameHandlerTwice_UnsubscribeOnceStillReceives()
        {
            var count = 0;
            Action<TestEvent> handler = _ => count++;

            _bus.Subscribe(handler);
            _bus.Subscribe(handler);
            _bus.Publish(new TestEvent(1));
            Assert.AreEqual(2, count);

            _bus.Unsubscribe(handler);
            _bus.Publish(new TestEvent(2));
            Assert.AreEqual(3, count);
        }

        private readonly struct TestEvent : IEvent
        {
            public readonly int Value;

            public TestEvent(int value)
            {
                Value = value;
            }
        }

        private readonly struct OtherTestEvent : IEvent
        {
            public readonly int Value;

            public OtherTestEvent(int value)
            {
                Value = value;
            }
        }
    }
}
