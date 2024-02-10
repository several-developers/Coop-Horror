using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Levels.GameTime;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.UI;
using GameCore.Observers.Global.Graphy;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Sun _sun;

        [SerializeField, Required]
        private MobileHeadquartersEntity _mobileHeadquartersEntity;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
            BindPlayerInteractionObserver();
            BindDungeonsObserver();
            BindGraphyStateObserver();
            BindMobileHeadquartersEntity();
            BindSun();
            BindTimeCycle();
            BindLocationsLoader();
            BindLevelManager();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle();
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

        private void BindGraphyStateObserver()
        {
            Container
                .BindInterfacesTo<GraphyStateObserver>()
                .AsSingle();
        }

        private void BindMobileHeadquartersEntity()
        {
            Container
                .Bind<IMobileHeadquartersEntity>()
                .FromInstance(_mobileHeadquartersEntity)
                .AsSingle();
        }

        private void BindSun()
        {
            Container
                .Bind<Sun>()
                .FromInstance(_sun)
                .AsSingle();
        }

        private void BindTimeCycle()
        {
            Container
                .BindInterfacesTo<TimeCycle>()
                .AsSingle();
        }

        private void BindLocationsLoader()
        {
            Container
                .BindInterfacesTo<LocationsLoader>()
                .AsSingle();
        }

        private void BindLevelManager()
        {
            Container
                .BindInterfacesTo<LevelManager>()
                .AsSingle();
        }
    }
}