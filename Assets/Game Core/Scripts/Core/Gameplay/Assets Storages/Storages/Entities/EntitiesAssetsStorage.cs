using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network.PrefabsRegistrar;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.AssetsStorages
{
    public class EntitiesAssetsStorage : AssetsStorage<Type>, IEntitiesAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesAssetsStorage(
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            INetworkPrefabsRegistrar networkPrefabsRegistrar
        ) : base(assetsProvider)
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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<AssetReferenceGameObject> allDynamicReferences = _entitiesListConfig.GetAllDynamicReferences();
            IEnumerable<AssetReferenceGameObject> allGlobalReferences = _entitiesListConfig.GetAllGlobalReferences();

            foreach (AssetReferenceGameObject assetReference in allDynamicReferences)
            {
                var entity = await LoadAndReleaseAsset<Entity>(assetReference);
                Type entityType = entity.GetType();

                AddDynamicAsset(entityType, assetReference);
            }

            foreach (AssetReferenceGameObject assetReference in allGlobalReferences)
            {
                // (Type type, GameObject gameObject) result = await GetAssetAfterLoadAndRelease(assetReference);
                // AddAsset(result.type, assetReference);
                // _networkPrefabsRegistrar.Register(result.gameObject);
                
                var entity = await LoadAsset<Entity>(assetReference);
                Type entityType = entity.GetType();
                
                AddAsset(entityType, assetReference);
                _networkPrefabsRegistrar.Register(entity.gameObject);
            }
        }
    }
}