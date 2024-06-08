using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.UIManagement;
using GameCore.Gameplay.VisualManagement;
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
        private MobileHeadquartersEntity _mobileHeadquartersEntity;

        [SerializeField, Required]
        private DeliveryPoint _deliveryPoint;

        [SerializeField, Required]
        private PlayerCamera _playerCamera;

        [SerializeField, Required]
        private SpectatorCamera _spectatorCamera;
        
        [SerializeField, Required]
        private VisualManager _visualManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMobileHeadquartersEntity();
            BindDeliveryPoint();
            BindTimeCycle();
            BindLocationsLoader();
            BindLevelProvider();
            BindDungeonsManager();
            BindFireExitsManager();
            BindLocationManagerDecorator();
            BindUIManager();
            BindVisualManager();
            BindPlayerCamera();
            BindSpectatorCamera();
            BindCamerasManager();
            BindGameResetManager();
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

        private void BindTimeCycle()
        {
            Container
                .BindInterfacesTo<TimeCycle>()
                .AsSingle()
                .NonLazy();
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
                .BindInterfacesAndSelfTo<DungeonsManager>()
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

        private void BindUIManager()
        {
            Container
                .BindInterfacesTo<UIManager>()
                .AsSingle()
                .NonLazy();
        }

        private void BindVisualManager()
        {
            Container
                .Bind<IVisualManager>()
                .FromInstance(_visualManager)
                .AsSingle();
        }

        private void BindPlayerCamera()
        {
            Container
                .Bind<PlayerCamera>()
                .FromInstance(_playerCamera)
                .AsSingle();
        }

        private void BindSpectatorCamera()
        {
            Container
                .Bind<SpectatorCamera>()
                .FromInstance(_spectatorCamera)
                .AsSingle();
        }

        private void BindCamerasManager()
        {
            Container
                .BindInterfacesTo<CamerasManager>()
                .AsSingle();
        }

        private void BindGameResetManager()
        {
            Container
                .BindInterfacesTo<GameResetManager>()
                .AsSingle()
                .NonLazy();
        }
    }
}