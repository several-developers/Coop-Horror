﻿using GameCore.Gameplay.AssetsStorages;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class AssetsStoragesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMenusAssetsStorage();
            BindEntitiesAssetsStorage();
            BindMonstersAssetsStorage();
            BindItemsAssetsStorage();
            BindItemsPreviewAssetsStorage();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindMenusAssetsStorage()
        {
            Container
                .BindInterfacesTo<MenusAssetsStorage>()
                .AsSingle();
        }
        private void BindEntitiesAssetsStorage()
        {
            Container
                .BindInterfacesTo<EntitiesAssetsStorage>()
                .AsSingle();
        }
        private void BindMonstersAssetsStorage()
        {
            Container
                .BindInterfacesTo<MonstersAssetsStorage>()
                .AsSingle();
        }
        
        private void BindItemsAssetsStorage()
        {
            Container
                .BindInterfacesTo<ItemsAssetsStorage>()
                .AsSingle();
        }
        
        private void BindItemsPreviewAssetsStorage()
        {
            Container
                .BindInterfacesTo<ItemsPreviewAssetsStorage>()
                .AsSingle();
        }
    }
}