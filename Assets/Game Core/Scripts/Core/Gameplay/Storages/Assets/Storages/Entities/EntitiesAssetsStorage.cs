using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public class EntitiesAssetsStorage : AssetsStorage<Type>, IEntitiesAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesAssetsStorage(IAssetsProvider assetsProvider, IConfigsProvider configsProvider)
            : base(assetsProvider)
        {
            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EntitiesListConfigMeta _entitiesListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<AssetReferenceGameObject> allDynamicReferences = _entitiesListConfig.GetAllDynamicReferences();

            foreach (AssetReferenceGameObject assetReference in allDynamicReferences)
            {
                var entity = await LoadAndReleaseAsset<Entity>(assetReference);
                Type entityType = entity.GetType();

                AddDynamicAsset(entityType, assetReference);
            }
        }
    }
}