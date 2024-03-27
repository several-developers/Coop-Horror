using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.GameTime;
using GameCore.Gameplay.Levels.Locations;
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
            BindMobileHeadquartersEntity();
            BindSun();
            BindTimeCycle();
            BindLocationsLoader();
            BindLevelManager();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

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
                .AsSingle()
                .NonLazy();
        }
    }
}