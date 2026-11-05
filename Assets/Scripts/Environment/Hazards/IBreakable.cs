namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Interface for destructible environmental objects on the battlefield (e.g. Emerald Meadow Ancient Bushes).
    /// </summary>
    public interface IBreakable
    {
        bool IsDestroyed { get; }
        void TakeBreakableDamage(float amount);
    }
}
