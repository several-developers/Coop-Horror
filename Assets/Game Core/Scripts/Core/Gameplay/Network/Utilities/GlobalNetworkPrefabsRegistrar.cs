using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Network.PrefabsRegistrar;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Network.Utilities
{
    public class GlobalNetworkPrefabsRegistrar
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GlobalNetworkPrefabsRegistrar(
            INetworkPrefabsRegistrar networkPrefabsRegistrar,
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider
        )
        {
            _networkPrefabsRegistrar = networkPrefabsRegistrar;
            _assetsProvider = assetsProvider;
            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly INetworkPrefabsRegistrar _networkPrefabsRegistrar;
        private readonly IAssetsProvider _assetsProvider;
        private readonly EntitiesListConfigMeta _entitiesListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask RegisterPrefabs()
        {
            await RegisterEntities();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask RegisterEntities()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllGlobalReferences();

            foreach (AssetReferenceGameObject assetReference in allReferences)
                await LoadAndRegisterAsset(assetReference);
        }

        private async UniTask LoadAndRegisterAsset(AssetReferenceGameObject assetReference)
        {
            var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            _networkPrefabsRegistrar.Register(prefab);
            //_assetsProvider.ReleaseAsset(assetReference);
        }
    }
}