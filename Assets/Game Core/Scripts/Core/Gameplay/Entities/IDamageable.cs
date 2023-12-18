namespace GameCore.Gameplay.Entities
{
    public interface IDamageable
    {
        void TakeDamage(IEntity source, float damage);
        bool IsDead();
    }
}