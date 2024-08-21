namespace GameCore.Gameplay.Entities
{
    public interface IDamageable
    {
        void TakeDamage(float damage, IEntity source = null);
        void KillInstant();
        bool IsDead();
    }
}