using System;
using System.Collections.Generic;
using GameCore.Configs.Global.MenuPrefabsList;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using GameCore.UI.Global.Other;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories
{
    public class MenuStaticFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenuStaticFactory(DiContainer diContainer, IAssetsProvider assetsProvider)
        {
            _diContainer = diContainer;
            _menusDictionary = new Dictionary<Type, MenuView>();

            SetupMenuDictionary(assetsProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static DiContainer _diContainer;
        private static Dictionary<Type, MenuView> _menusDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void Create(Type menuType)
        {
            if (!_menusDictionary.ContainsKey(menuType))
                return;

            MenuView menuView = _menusDictionary[menuType];
            InstantiatePrefab(menuView, MainCanvas.Transform, _diContainer);
        }

        public static void Create<TPayload>(Type menuType, TPayload param)
        {
            if (!_menusDictionary.ContainsKey(menuType))
                return;

            MenuView menuView = _menusDictionary[menuType];
            GameObject menuInstance = InstantiatePrefab(menuView, MainCanvas.Transform, _diContainer);

            if (menuInstance.TryGetComponent(out IComplexMenuView<TPayload> component))
                component.Setup(param);
        }

        public static TMenu Create<TMenu>() where TMenu : MenuView =>
            Create<TMenu>(MainCanvas.Transform);

        public static TMenu Create<TMenu>(DiContainer diContainer) where TMenu : MenuView =>
            Create<TMenu>(MainCanvas.Transform, diContainer);

        public static TMenu Create<TMenu>(Transform container) where TMenu : MenuView =>
            Create<TMenu>(container, _diContainer);

        public static TMenu Create<TMenu>(Transform container, DiContainer diContainer) where TMenu : MenuView =>
            InstantiatePrefabForComponent<TMenu>(container, diContainer);

        public static void Create<TMenu, TPayload>(TPayload param)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            Create<TMenu, TPayload>(MainCanvas.Transform, param, _diContainer);
        }

        public static void Create<TMenu, TPayload>(TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            Create<TMenu, TPayload>(MainCanvas.Transform, param, diContainer);
        }

        public static void Create<TMenu, TPayload>(Transform container, TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>
        {
            var menuInstance = InstantiatePrefabForComponent<TMenu>(container, diContainer);
            menuInstance.Setup(param);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void SetupMenuDictionary(IAssetsProvider assetsProvider)
        {
            MenuPrefabsListConfigMeta menuPrefabsListConfigMeta = assetsProvider.GetMenuPrefabsListConfig();

            if (menuPrefabsListConfigMeta == null)
                return;

            MenuView[] menuPrefabs = menuPrefabsListConfigMeta.GetMenuPrefabs();

            foreach (MenuView menuPrefab in menuPrefabs)
            {
                if (CheckForNull(menuPrefab))
                    continue;

                Type type = menuPrefab.GetType();
                _menusDictionary.Add(type, menuPrefab);
            }

            bool CheckForNull(MenuView menuPrefab)
            {
                if (menuPrefab != null)
                    return false;

                Debug.LogError("Missing <gb>menu prefab</gb> reference!");

                return true;
            }
        }

        private static GameObject InstantiatePrefab(MenuView menuView, Transform container, DiContainer diContainer) =>
            diContainer.InstantiatePrefab(menuView, container);

        private static TMenu InstantiatePrefabForComponent<TMenu>(Transform container, DiContainer diContainer)
            where TMenu : MenuView
        {
            var menu = GetMenu<TMenu>();
            return diContainer.InstantiatePrefabForComponent<TMenu>(menu, container);
        }

        private static TMenu GetMenu<TMenu>() where TMenu : MenuView, IMenuView =>
            _menusDictionary[typeof(TMenu)] as TMenu;
    }
}