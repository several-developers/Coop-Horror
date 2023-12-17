using GameCore.Gameplay;
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
            DiContainer diContainer)
        {
            _gameStateMachine = gameStateMachine;
            _assetsProvider = assetsProvider;
            _diContainer = diContainer;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAssetsProvider _assetsProvider;
        private readonly DiContainer _diContainer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateScenesLoader();
            CreateNetworkManager();
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
            Object.Instantiate(networkManagerPrefab);
        }

        private void EnterLoadDataState() =>
            _gameStateMachine.ChangeState<LoadDataState>();
    }
}