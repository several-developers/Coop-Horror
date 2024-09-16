using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Generators.Monsters;
using GameCore.Gameplay.Items.Generators.Dungeon;
using GameCore.Gameplay.Items.Generators.OutdoorChest;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network.Utilities;
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
        private DeathCamera _deathCamera;

        [SerializeField, Required]
        private SpectatorCamera _spectatorCamera;

        [SerializeField, Required]
        private VisualManager _visualManager;

        [SerializeField, Required]
        private Sun _sun;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            // Костылик :(
            ZenjectNetCodeFactory.StaticDIContainer = Container;

            BindMobileHeadquartersEntity();
            BindTimeService();
            BindLocationsLoader();
            BindLevelProvider();
            BindDungeonsManager();
            BindFireExitsManager();
            BindUIManager();
            BindVisualManager();
            BindSun();
            BindPlayerCamera();
            BindDeathCamera();
            BindSpectatorCamera();
            BindCamerasManager();
            BindGameResetManager();
            BindDungeonItemsGenerator();
            BindOutdoorChestItemsGenerator();
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

        private void BindTimeService()
        {
            Container
                .BindInterfacesTo<TimeService>()
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

        private void BindSun()
        {
            Container
                .Bind<Sun>()
                .FromInstance(_sun)
                .AsSingle();
        }

        private void BindPlayerCamera()
        {
            Container
                .Bind<PlayerCamera>()
                .FromInstance(_playerCamera)
                .AsSingle();
        }

        private void BindDeathCamera()
        {
            Container
                .Bind<DeathCamera>()
                .FromInstance(_deathCamera)
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

        private void BindDungeonItemsGenerator()
        {
            Container
                .BindInterfacesAndSelfTo<DungeonItemsGenerator>()
                .AsSingle();

        }
        
        private void BindOutdoorChestItemsGenerator()
        {
            Container
                .BindInterfacesAndSelfTo<OutdoorChestItemsGenerator>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameEventsHandler()
        {
            Container
                .BindInterfacesTo<GameFlowController>()
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
                .AsSingle();
        }
    }
}