namespace FrogGame.Core
{
    /// <summary>
    /// Interface for entities or objects that can receive damage.
    /// Interfaz para cualquier objeto o enemigo que pueda recibir daño.
    /// </summary>
    public interface IDamageable
    {
        public void TakeDamage(int damageAmount);
    }
}