using System;
using System.Collections.Generic;
using NUnit.Framework;
using Core.StateMachine;

namespace Tests.Core
{
    [TestFixture]
    public class StateMachineTests
    {
        private object _owner;

        [SetUp]
        public void SetUp()
        {
            _owner = new object();
        }

        [Test]
        public void Constructor_NullInitialState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new StateMachine<object>(_owner, null));
        }

        [Test]
        public void Constructor_CallsEnterAndPassesOwnerToInitialState()
        {
            var initialState = new RecordingState("A");

            _ = new StateMachine<object>(_owner, initialState);

            Assert.AreEqual(1, initialState.EnterCount);
            Assert.AreEqual(0, initialState.ExitCount);
            Assert.AreEqual(0, initialState.TickCount);
            Assert.AreSame(_owner, initialState.ReceivedOwner);
        }

        [Test]
        public void Constructor_SetsCurrentState()
        {
            var initialState = new RecordingState("A");
            var machine = new StateMachine<object>(_owner, initialState);

            Assert.AreSame(initialState, machine.CurrentState);
        }

        [Test]
        public void ChangeState_Null_ThrowsArgumentNullException()
        {
            var machine = new StateMachine<object>(_owner, new RecordingState("A"));

            Assert.Throws<ArgumentNullException>(() => machine.ChangeState(null));
        }

        [Test]
        public void ChangeState_DifferentState_ExitThenEnterAndTickOnlyCurrentState()
        {
            var stateA = new RecordingState("A");
            var stateB = new RecordingState("B");
            var machine = new StateMachine<object>(_owner, stateA);

            machine.ChangeState(stateB);

            Assert.AreEqual(1, stateA.ExitCount);
            Assert.AreEqual(1, stateB.EnterCount);
            Assert.AreSame(_owner, stateB.ReceivedOwner);
            Assert.AreSame(stateB, machine.CurrentState);

            machine.Tick(0.1f);

            Assert.AreEqual(0, stateA.TickCount);
            Assert.AreEqual(1, stateB.TickCount);
        }

        [Test]
        public void ChangeState_MultipleTransitions_ExitEnterEachIntermediateState()
        {
            var stateA = new RecordingState("A");
            var stateB = new RecordingState("B");
            var stateC = new RecordingState("C");
            var machine = new StateMachine<object>(_owner, stateA);

            machine.ChangeState(stateB);
            machine.ChangeState(stateC);

            Assert.AreSame(stateC, machine.CurrentState);
            Assert.AreEqual(1, stateA.ExitCount);
            Assert.AreEqual(1, stateB.EnterCount);
            Assert.AreEqual(1, stateB.ExitCount);
            Assert.AreEqual(1, stateC.EnterCount);
            Assert.AreEqual(0, stateC.ExitCount);
        }

        [Test]
        public void ChangeState_SameInstance_NoOp()
        {
            var stateA = new RecordingState("A");
            var machine = new StateMachine<object>(_owner, stateA);

            machine.ChangeState(stateA);

            Assert.AreEqual(1, stateA.EnterCount);
            Assert.AreEqual(0, stateA.ExitCount);
        }

        [Test]
        public void Tick_ForwardsDeltaTime()
        {
            var state = new RecordingState("A");
            var machine = new StateMachine<object>(_owner, state);

            machine.Tick(0.16f);

            Assert.AreEqual(1, state.TickCount);
            Assert.AreEqual(0.16f, state.LastDeltaTime);
            Assert.AreSame(_owner, state.ReceivedOwner);
        }

        [Test]
        public void FullFlow_RecordsLifecycleOrder()
        {
            var events = new List<string>();
            var stateA = new RecordingState("A", events);
            var stateB = new RecordingState("B", events);
            var machine = new StateMachine<object>(_owner, stateA);

            machine.Tick(0.1f);
            machine.ChangeState(stateB);
            machine.Tick(0.2f);

            CollectionAssert.AreEqual(
                new[] { "A.Enter", "A.Tick", "A.Exit", "B.Enter", "B.Tick" },
                events);
        }

        private sealed class RecordingState : IState<object>
        {
            private readonly string _name;
            private readonly List<string> _events;

            public int EnterCount { get; private set; }
            public int TickCount { get; private set; }
            public int ExitCount { get; private set; }
            public object ReceivedOwner { get; private set; }
            public float LastDeltaTime { get; private set; }

            public RecordingState(string name, List<string> events = null)
            {
                _name = name;
                _events = events;
            }

            public void Enter(object owner)
            {
                EnterCount++;
                ReceivedOwner = owner;
                _events?.Add($"{_name}.Enter");
            }

            public void Tick(object owner, float deltaTime)
            {
                TickCount++;
                ReceivedOwner = owner;
                LastDeltaTime = deltaTime;
                _events?.Add($"{_name}.Tick");
            }

            public void Exit(object owner)
            {
                ExitCount++;
                ReceivedOwner = owner;
                _events?.Add($"{_name}.Exit");
            }
        }
    }
}
