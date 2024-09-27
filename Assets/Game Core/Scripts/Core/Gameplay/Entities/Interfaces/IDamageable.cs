namespace GameCore.Gameplay.Entities
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        bool IsDead();
    }
}