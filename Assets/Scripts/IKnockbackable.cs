namespace FrogGame.Core
{
    using UnityEngine;

    /// <summary>
    /// Interface for entities that can receive temporary knockback impulses.
    /// </summary>
    public interface IKnockbackable
    {
        public void ApplyKnockback(Vector2 direction, float force, float duration = 0.15f);
    }
}