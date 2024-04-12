using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Dungeons;
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

        [SerializeField, Required]
        private DeliveryPoint _deliveryPoint;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMobileHeadquartersEntity();
            BindDeliveryPoint();
            BindSun();
            BindTimeCycle();
            BindLocationsLoader();
            BindLevelProvider();
            BindDungeonsManager();
            BindFireExitsManager();
            BindLocationManagerDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindMobileHeadquartersEntity()
        {
            Container
                .Bind<IMobileHeadquartersEntity>()
                .FromInstance(_mobileHeadquartersEntity)
                .AsSingle();
        }
        
        private void BindDeliveryPoint()
        {
            Container
                .Bind<IDeliveryPoint>()
                .FromInstance(_deliveryPoint)
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

        private void BindLevelProvider()
        {
            Container
                .BindInterfacesTo<LevelProvider>()
                .AsSingle()
                .NonLazy();
        }

        private void BindDungeonsManager()
        {
            Container
                .BindInterfacesTo<DungeonsManager>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindFireExitsManager()
        {
            Container
                .BindInterfacesTo<FireExitsManager>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindLocationManagerDecorator()
        {
            Container
                .BindInterfacesTo<LocationManagerDecorator>()
                .AsSingle();
        }
    }
}