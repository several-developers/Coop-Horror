using GameCore.Gameplay.Locations.GameTime;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Global;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.StateMachine
{
    //  -----------------------------------------
    //
    //               STATES FLOW:
    //  Bootstrap -> Load Data -> Load Main Menu
    //
    //  -----------------------------------------

    public class BootstrapState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public BootstrapState(IGameStateMachine gameStateMachine, IAssetsProvider assetsProvider,
            ITimeCycleDecorator timeCycleDecorator, DiContainer diContainer)
        {
            _gameStateMachine = gameStateMachine;
            _assetsProvider = assetsProvider;
            _timeCycleDecorator = timeCycleDecorator; // TEMP
            _diContainer = diContainer;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAssetsProvider _assetsProvider;
        private readonly ITimeCycleDecorator _timeCycleDecorator; // TEMP
        private readonly DiContainer _diContainer;

        private NetworkManager _networkManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateScenesLoader();
            CreateNetworkManager();
            CreateNetworkHorror();
            EnterLoadDataState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateScenesLoader()
        {
            GameObject scenesLoaderPrefab = _assetsProvider.GetScenesLoaderPrefab();
            _diContainer.InstantiatePrefab(scenesLoaderPrefab);
        }

        private void CreateNetworkManager()
        {
            NetworkManager networkManagerPrefab = _assetsProvider.GetNetworkManager();
            _networkManager = Object.Instantiate(networkManagerPrefab);
        }

        private void CreateNetworkHorror()
        {
            TheNetworkHorror networkHorror = _assetsProvider.GetNetworkHorror();
            TheNetworkHorror networkHorrorInstance = Object.Instantiate(networkHorror);
            networkHorrorInstance.Init(_networkManager, _timeCycleDecorator);
        }

        private void EnterLoadDataState() =>
            _gameStateMachine.ChangeState<LoadDataState>();
    }
}