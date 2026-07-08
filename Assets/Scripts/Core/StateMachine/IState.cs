namespace Core.StateMachine
{
    /// <summary>
    /// Lifecycle contract for a state managed by a <see cref="StateMachine{TOwner}"/>.
    /// </summary>
    /// <typeparam name="TOwner">The owner context associated with this state machine.</typeparam>
    public interface IState<TOwner>
    {
        /// <summary>
        /// Called once when entering this state.
        /// </summary>
        /// <param name="owner">The owner context associated with this state machine.</param>
        void Enter(TOwner owner);

        /// <summary>
        /// Called repeatedly while this state is active.
        /// </summary>
        /// <param name="owner">The owner context associated with this state machine</param>
        /// <param name="deltaTime">Time elapsed since the previous update step, in seconds.</param>
        void Tick(TOwner owner, float deltaTime);

        /// <summary>
        /// Called once when leaving this state.
        /// </summary>
        /// <param name="owner">The owner context associated with this state machine.</param>
        void Exit(TOwner owner);
    }
}
