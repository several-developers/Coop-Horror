using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Utilities;

namespace GameCore.Gameplay.Factories.Entities
{
    public interface IEntitiesFactory
    {
        UniTask CreateEntity<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity;
        void CreateEntityDynamic<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity;
    }
}