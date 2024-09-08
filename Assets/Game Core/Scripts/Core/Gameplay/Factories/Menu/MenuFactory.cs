using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.MenuPrefabsList;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using GameCore.UI.Global.Other;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories.Menu
{
    public class MenuFactory : AddressablesFactoryBase<Type>, IMenuFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenuFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider, 
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            IConfigsProvider configsProvider
            ) : base(diContainer, assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _diContainer = diContainer;
            _configsProvider = configsProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IConfigsProvider _configsProvider;
        private readonly DiContainer _diContainer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTask<GameObject> Create(Type menuType) =>
            await InstantiatePrefab(menuType, MainCanvas.Transform, _diContainer);

        public async UniTask<GameObject> Create<TPayload>(Type menuType, TPayload param)
        {
            GameObject menuInstance = await InstantiatePrefab(menuType, MainCanvas.Transform, _diContainer);

            if (menuInstance.TryGetComponent(out IComplexMenuView<TPayload> component))
                component.Setup(param);

            return menuInstance;
        }

        public async UniTask<TMenu> Create<TMenu>() where TMenu : MenuView =>
            await Create<TMenu>(MainCanvas.Transform);

        public async UniTask<TMenu> Create<TMenu>(DiContainer diContainer) where TMenu : MenuView =>
            await Create<TMenu>(MainCanvas.Transform, diContainer);

        public async UniTask<TMenu> Create<TMenu>(Transform container) where TMenu : MenuView =>
            await Create<TMenu>(container, _diContainer);

        public async UniTask<TMenu> Create<TMenu>(Transform container, DiContainer diContainer)
            where TMenu : MenuView
        {
            return await InstantiatePrefabForComponent<TMenu>(container, diContainer);
        }

        public async UniTask<TMenu> Create<TMenu, TPayload>(TPayload param)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            return await Create<TMenu, TPayload>(MainCanvas.Transform, param, _diContainer);
        }

        public async UniTask<TMenu> Create<TMenu, TPayload>(TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            return await Create<TMenu, TPayload>(MainCanvas.Transform, param, diContainer);
        }

        public async UniTask<TMenu> Create<TMenu, TPayload>(Transform container, TPayload param,
            DiContainer diContainer) where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            var menuInstance = await InstantiatePrefabForComponent<TMenu>(container, diContainer);
            menuInstance.Setup(param);

            return menuInstance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            var menuPrefabsListConfig = _configsProvider.GetConfig<MenuPrefabsListConfigMeta>();

            if (menuPrefabsListConfig == null)
                return;

            IEnumerable<AssetReferenceGameObject> allMenuReferences = menuPrefabsListConfig.GetAllMenuReferences();

            foreach (AssetReferenceGameObject assetReference in allMenuReferences)
            {
                var entity = await LoadAndReleaseAsset<MenuView>(assetReference);
                Type key = entity.GetType();
                
                AddAsset(key, assetReference);
            }
        }

#warning Maybe working not correctly with Zenject, need to be checked.
        private async UniTask<GameObject> InstantiatePrefab(Type menuType, Transform container, DiContainer diContainer)
        {
            bool isAssetReferenceFound = TryGetAssetReference(menuType, out AssetReference assetReference);

            if (!isAssetReferenceFound)
                return null;

            var menuPrefab = await LoadAsset<MenuView>(assetReference);
            return diContainer.InstantiatePrefab(menuPrefab, container);
        }

#warning Maybe working not correctly with Zenject, need to be checked.
        private async UniTask<TMenu> InstantiatePrefabForComponent<TMenu>(Transform container, DiContainer diContainer)
            where TMenu : MenuView
        {
            Type menuType = typeof(TMenu);
            var menuPrefab = await LoadAsset<TMenu>(menuType);
            return diContainer.InstantiatePrefabForComponent<TMenu>(menuPrefab, container);
        }
    }
}