using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.AssetsStorages;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using GameCore.UI.Global.Other;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories.Menu
{
    public class MenusFactory : AddressablesFactoryBase<Type>, IMenusFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenusFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IMenusAssetsStorage assetsStorage,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(diContainer, assetsProvider, assetsStorage, dynamicPrefabsLoaderDecorator)
        {
            _diContainer = diContainer;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DiContainer _diContainer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

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

        public async UniTask<TMenu> Create<TMenu>(Transform parent) where TMenu : MenuView =>
            await Create<TMenu>(parent, _diContainer);

        public async UniTask<TMenu> Create<TMenu>(Transform parent, DiContainer diContainer)
            where TMenu : MenuView
        {
            return await InstantiatePrefabForComponent<TMenu>(parent, diContainer);
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

        public async UniTask<TMenu> Create<TMenu, TPayload>(Transform parent, TPayload param,
            DiContainer diContainer) where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            var menuInstance = await InstantiatePrefabForComponent<TMenu>(parent, diContainer);
            menuInstance.Setup(param);

            return menuInstance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask<GameObject> InstantiatePrefab(Type menuType, Transform parent, DiContainer diContainer)
        {
            var spawnParams = new SpawnParams<MenuView>.Builder()
                .SetParent(parent)
                .Build();

            return await LoadAndCreateGameObject(menuType, spawnParams, diContainer);
        }

        private async UniTask<TMenu> InstantiatePrefabForComponent<TMenu>(Transform parent, DiContainer diContainer)
            where TMenu : MenuView
        {
            Type menuType = typeof(TMenu);
            GameObject menuPrefab2 = await InstantiatePrefab(menuType, parent, diContainer);
            
            return menuPrefab2.GetComponent<TMenu>();
        }
    }
}