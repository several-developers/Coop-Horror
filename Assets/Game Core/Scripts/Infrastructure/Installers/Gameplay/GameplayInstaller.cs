using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Items.SpawnSystem;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.MonstersGeneration;
using GameCore.Gameplay.RoundManagement;
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
        private TrainEntity _trainEntity;

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
            BindTimeCycle();
            BindLocationsLoader();
            BindLevelProvider();
            BindDungeonsManager();
            BindFireExitsManager();
            BindMetroManager();
            BindUIManager();
            BindVisualManager();
            BindPlayerCamera();
            BindSpectatorCamera();
            BindCamerasManager();
            BindGameResetManager();
            BindItemsSpawnSystem();
            BindGameEventsHandler();
            BindRoundManager();
            BindMonstersGenerator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindMobileHeadquartersEntity()
        {
            Container
                .Bind<ITrainEntity>()
                .FromInstance(_trainEntity)
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
                .AsSingle();
        }

        private void BindMetroManager()
        {
            Container
                .BindInterfacesTo<MetroManager>()
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
                .AsSingle();
        }

        private void BindItemsSpawnSystem()
        {
            Container
                .BindInterfacesTo<ItemsSpawnSystem>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameEventsHandler()
        {
            Container
                .BindInterfacesTo<GameController>()
                .AsSingle()
                .NonLazy();
        }

        private void BindRoundManager()
        {
            Container
                .BindInterfacesTo<RoundManager>()
                .AsSingle()
                .NonLazy();
        }

        private void BindMonstersGenerator()
        {
            Container
                .BindInterfacesTo<MonstersGenerator>()
                .AsSingle()
                .NonLazy();
        }
    }
}