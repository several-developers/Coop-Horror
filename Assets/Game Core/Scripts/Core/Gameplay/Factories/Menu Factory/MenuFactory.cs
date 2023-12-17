using System;
using System.Collections.Generic;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Global.MenuView;
using GameCore.UI.Global.Other;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories
{
    public class MenuFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MenuFactory(DiContainer diContainer, IAssetsProvider assetsProvider)
        {
            _diContainer = diContainer;
            _menusDictionary = new Dictionary<Type, MenuView>(capacity: 16);

            SetupMenuDictionary(assetsProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static DiContainer _diContainer;
        private static Dictionary<Type, MenuView> _menusDictionary;
        private static Dictionary<Type, MenuView> _menusInstancesDictionary;

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

        public static TMenu Create<TMenu>(Transform container) where TMenu : MenuView
        {
            TMenu menuPrefab = GetMenu<TMenu>();
            TMenu menuInstance = InstantiatePrefabForComponent(menuPrefab, container, _diContainer);
            return menuInstance;
        }
        
        public static TMenu Create<TMenu>(Transform container, DiContainer diContainer) where TMenu : MenuView
        {
            TMenu menu = GetMenu<TMenu>();
            return InstantiatePrefabForComponent(menu, container, diContainer);
        }

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
            TMenu menu = GetMenu<TMenu>();
            TMenu menuInstance = InstantiatePrefabForComponent<TMenu>(menu, container, diContainer);
            menuInstance.Setup(param);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void SetupMenuDictionary(IAssetsProvider assetsProvider)
        {
            MenuPrefabsListMeta menuPrefabsListMeta = assetsProvider.GetMenuPrefabsList();
            
            if (menuPrefabsListMeta == null)
                return;
            
            MenuView[] menuPrefabs = menuPrefabsListMeta.GetMenuPrefabs();

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

        private static TMenu GetMenu<TMenu>() where TMenu : MenuView, IMenuView =>
            _menusDictionary[typeof(TMenu)] as TMenu;

        private static GameObject InstantiatePrefab(MenuView menuView, Transform container, DiContainer diContainer) =>
            diContainer.InstantiatePrefab(menuView, container);

        private static TMenu InstantiatePrefabForComponent<TMenu>(TMenu menu, Transform container,
            DiContainer diContainer) where TMenu : MenuView
        {
            return diContainer.InstantiatePrefabForComponent<TMenu>(menu, container);
        }
    }
}