using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.MenuPrefabsList;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using GameCore.UI.Global.Other;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories.Menu
{
    public class MenuFactory : IMenuFactory, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenuFactory(DiContainer diContainer, IAssetsProvider assetsProvider)
        {
            _diContainer = diContainer;
            _assetsProvider = assetsProvider;
            _referencesDictionary = new Dictionary<Type, AssetReferenceGameObject>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetsProvider;
        private readonly Dictionary<Type, AssetReferenceGameObject> _referencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize() => SetupReferencesDictionary();

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
            DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            var menuInstance = await InstantiatePrefabForComponent<TMenu>(container, diContainer);
            menuInstance.Setup(param);

            return menuInstance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupReferencesDictionary()
        {
            MenuPrefabsListConfigMeta menuPrefabsListConfig = _assetsProvider.GetMenuPrefabsListConfig();

            if (menuPrefabsListConfig == null)
                return;

            IEnumerable<AssetReferenceGameObject> allMenuReferences = menuPrefabsListConfig.GetAllMenuReferences();

            foreach (AssetReferenceGameObject assetReference in allMenuReferences)
            {
                if (!assetReference.editorAsset.TryGetComponent(out MenuView menuView))
                {
                    Log.PrintError(log: $"<gb>Entity '{assetReference.AssetGUID}' asset</gb> was <rb>not found</rb>!");
                    continue;
                }

                Type type = menuView.GetType();
                bool success = _referencesDictionary.TryAdd(type, assetReference);

                if (success)
                    continue;

                Log.PrintError(log: $"Key '<gb>{type.Name}</gb>' <rb>already exists</rb>!");
            }
        }

        private AssetReferenceGameObject GetMenuReference<TMenu>() where TMenu : MenuView, IMenuView
        {
            Type key = typeof(TMenu);

            bool isAssetReferenceFound =
                _referencesDictionary.TryGetValue(key, out AssetReferenceGameObject assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference '<gb>{key.Name}</gb>' <rb>not found</rb>!");

            return assetReference;
        }

        private async UniTask<GameObject> InstantiatePrefab(Type menuType, Transform container, DiContainer diContainer)
        {
            bool isAssetReferenceFound =
                _referencesDictionary.TryGetValue(menuType, out AssetReferenceGameObject assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference '<gb>{menuType.Name}</gb>' <rb>not found</rb>!");

            MenuView menuPrefab = await LoadAsset(assetReference);
            return diContainer.InstantiatePrefab(menuPrefab, container);
        }

#warning Maybe working not correctly with Zenject, need to be checked.
        private async UniTask<TMenu> InstantiatePrefabForComponent<TMenu>(Transform container, DiContainer diContainer)
            where TMenu : MenuView
        {
            var menuPrefab = await LoadAsset<TMenu>();
            return diContainer.InstantiatePrefabForComponent<TMenu>(menuPrefab, container);
        }

        private async UniTask<TMenu> LoadAsset<TMenu>() where TMenu : MenuView
        {
            AssetReferenceGameObject assetReference = GetMenuReference<TMenu>();
            var menuPrefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            bool isMenuViewFound = menuPrefab.TryGetComponent(out TMenu menuView);

            if (!isMenuViewFound)
                Log.PrintError(log: "Menu View not found");

            return menuView;
        }

        private async UniTask<MenuView> LoadAsset(AssetReferenceGameObject assetReference)
        {
            var menuPrefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            bool isMenuViewFound = menuPrefab.TryGetComponent(out MenuView menuView);

            if (!isMenuViewFound)
                Log.PrintError(log: "Menu View not found");

            return menuView;
        }
    }
}