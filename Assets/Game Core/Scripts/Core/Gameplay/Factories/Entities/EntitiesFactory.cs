using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Storages.Assets;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using Zenject;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : AddressablesFactory<Type>, IEntitiesFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IEntitiesAssetsStorage assetsStorage,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(diContainer, assetsProvider, assetsStorage, dynamicPrefabsLoaderDecorator)
        {
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask CreateEntity<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            Type entityType = typeof(TEntity);

            await LoadAndCreateNetworkObject(entityType, spawnParams);
        }

        public void CreateEntityDynamic<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            Type entityType = typeof(TEntity);

            LoadAndCreateDynamicNetworkObject(entityType, spawnParams);
        }
    }
}