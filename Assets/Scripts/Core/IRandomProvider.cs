namespace Core
{
    /// <summary>
    /// Provides deterministic random values for gameplay systems.
    /// </summary>
    /// <remarks>
    /// Gameplay code should depend on this interface instead of
    /// <see cref="UnityEngine.Random"/> to improve testability and
    /// deterministic execution.
    /// </remarks>
    public interface IRandomProvider
    {
        /// <summary>
        /// Returns a random float in the range [0, 1).
        /// </summary>
        float NextFloat();
    }
}
