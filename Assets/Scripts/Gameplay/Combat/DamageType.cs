namespace Gameplay.Combat
{
    /// <summary>
    /// Defense category for the pipeline; crit is separate via <see cref="DamageContext.IsCrit"/>.
    /// </summary>
    public enum DamageType
    {
        Physical = 0,
        Magical,

        // Bypasses defense; crit still applies when DamageContext.IsCrit is true.
        True
    }
}