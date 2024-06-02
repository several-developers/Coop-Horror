namespace GameCore.Gameplay.Entities
{
    public interface IDamageable
    {
        void TakeDamage(float damage, IEntity source = null);
        bool IsDead();
    }
}