﻿using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Game;
using GameCore.Observers.Gameplay.Level;
using GameCore.Observers.Gameplay.LevelManager;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.Time;
using GameCore.Observers.Gameplay.UI;
using GameCore.Observers.Gameplay.UIManager;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class ObserversInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
            BindPlayerInteractionObserver();
            BindDungeonsObserver();
            BindLevelObserver();
            BindLevelProviderObserver();
            BindUIManagerObserver();
            BindGameObserver();
            BindTimeObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle()
                .NonLazy();
        }

        private void BindPlayerInteractionObserver()
        {
            Container
                .BindInterfacesTo<PlayerInteractionObserver>()
                .AsSingle();
        }

        private void BindDungeonsObserver()
        {
            Container
                .BindInterfacesTo<DungeonsObserver>()
                .AsSingle();
        }

        private void BindLevelObserver()
        {
            Container
                .BindInterfacesTo<LevelObserver>()
                .AsSingle();
        }

        private void BindLevelProviderObserver()
        {
            Container
                .BindInterfacesTo<LevelProviderObserver>()
                .AsSingle();
        }

        private void BindUIManagerObserver()
        {
            Container
                .BindInterfacesTo<UIManagerObserver>()
                .AsSingle();
        }

        private void BindGameObserver()
        {
            Container
                .BindInterfacesTo<GameObserver>()
                .AsSingle();
        }

        private void BindTimeObserver()
        {
            Container
                .BindInterfacesTo<TimeObserver>()
                .AsSingle();
        }
    }
}