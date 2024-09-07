using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Utilities;
using GameCore.Utilities;

namespace GameCore.Gameplay.Factories.Entities
{
    public interface IEntitiesFactory : IAddressablesFactory<Type>
    {
        UniTask WarmUp();
        UniTask CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity;
        void CreateEntityDynamic<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity;
    }
}