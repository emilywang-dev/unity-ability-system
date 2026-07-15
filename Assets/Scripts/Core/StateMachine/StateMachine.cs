using System;

namespace Core.StateMachine
{
    /// <summary>
    /// Drives state updates and transitions for a single owner context.
    /// </summary>
    /// <typeparam name="TOwner">The owner context associated with this state machine.</typeparam>
    public sealed class StateMachine<TOwner>
    {
        private readonly TOwner _owner;
        private IState<TOwner> _currentState;

        public IState<TOwner> CurrentState => _currentState;

        /// <summary>
        /// Creates a state machine and enters <paramref name="initialState"/> immediately.
        /// </summary>
        /// <param name="owner">The owner context associated with this state machine.</param>
        /// <param name="initialState">The first active state; its Enter method is invoked immediately.</param>
        /// <exception cref="ArgumentNullException"><paramref name="initialState"/> is null.</exception>
        public StateMachine(TOwner owner, IState<TOwner> initialState)
        {
            _owner = owner;
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            _currentState = initialState;
            _currentState.Enter(_owner);
        }

        /// <summary>
        /// Exits the current state and enters <paramref name="newState"/>.
        /// No-op when <paramref name="newState"/> is the same instance as <c>CurrentState</c>.
        /// </summary>
        /// <param name="newState">The state to activate after exiting the current state.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newState"/> is null.</exception>
        /// <remarks>
        /// Reentrant <c>ChangeState</c> calls from within
        /// <see cref="IState{TOwner}.Enter"/> or <see cref="IState{TOwner}.Exit"/>
        /// are not supported.
        /// Prefer initiating transitions from <see cref="IState{TOwner}.Tick"/>
        /// or external gameplay logic rather than lifecycle callbacks.
        /// Reentrant calls can result in unexpected lifecycle ordering.
        /// </remarks>
        public void ChangeState(IState<TOwner> newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            if (ReferenceEquals(newState, _currentState))
            {
                return;
            }

            var previous = _currentState;
            previous.Exit(_owner);
            _currentState = newState;
            _currentState.Enter(_owner);
        }

        /// <summary>
        /// Advances the current state by one update step.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the previous update step, in seconds.</param>
        public void Tick(float deltaTime)
        {
            _currentState.Tick(_owner, deltaTime);
        }
    }
}
