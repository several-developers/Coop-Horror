using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.PrefabsRegistrar;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : AddressablesFactoryBase<Type>, IEntitiesFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            INetworkPrefabsRegistrar networkPrefabsRegistrar,
            IConfigsProvider configsProvider
        ) : base(diContainer, assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _networkPrefabsRegistrar = networkPrefabsRegistrar;
            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly INetworkPrefabsRegistrar _networkPrefabsRegistrar;
        private readonly EntitiesListConfigMeta _entitiesListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllReferences();
            IEnumerable<AssetReferenceGameObject> allGlobalReferences = _entitiesListConfig.GetAllGlobalReferences();
            IEnumerable<AssetReferenceGameObject> allDynamicReferences = _entitiesListConfig.GetAllDynamicReferences();

            foreach (AssetReferenceGameObject assetReference in allReferences)
            {
                (Type type, GameObject gameObject) result = await GetAssetAfterLoadAndRelease(assetReference);
                
                AddAsset(result.type, assetReference);
            }

            foreach (AssetReferenceGameObject assetReference in allGlobalReferences)
            {
                (Type type, GameObject gameObject) result = await GetAssetAfterLoadAndRelease(assetReference);

                AddAsset(result.type, assetReference);
                _networkPrefabsRegistrar.Register(result.gameObject);
            }

            foreach (AssetReferenceGameObject assetReference in allDynamicReferences)
            {
                (Type type, GameObject gameObject) result = await GetAssetAfterLoadAndRelease(assetReference);
                
                AddDynamicAsset(result.type, assetReference);
            }

            // LOCAL METHODS: -----------------------------

            async UniTask<(Type type, GameObject gameObject)> GetAssetAfterLoadAndRelease(AssetReference assetReference)
            {
                var entity = await LoadAndReleaseAsset<Entity>(assetReference);
                Type entityType = entity.GetType();
                return (entityType, entity.gameObject);
            }
        }
    }
}