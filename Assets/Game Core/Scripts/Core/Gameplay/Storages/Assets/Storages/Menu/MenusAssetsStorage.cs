using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Configs.Global.MenuPrefabsList;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public class MenusAssetsStorage : AssetsStorage<Type>, IMenusAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenusAssetsStorage(IAssetsProvider assetsProvider, IConfigsProvider configsProvider) 
            : base(assetsProvider)
        {
            _menuPrefabsListConfig = configsProvider.GetConfig<MenuPrefabsListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MenuPrefabsListConfigMeta _menuPrefabsListConfig;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<AssetReferenceGameObject> allMenuReferences = _menuPrefabsListConfig.GetAllMenuReferences();

            foreach (AssetReferenceGameObject assetReference in allMenuReferences)
            {
                var entity = await LoadAndReleaseAsset<MenuView>(assetReference);
                Type key = entity.GetType();
                
                AddAsset(key, assetReference);
            }
        }
    }
}