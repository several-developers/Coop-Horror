using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Infrastructure.StateMachine
{
    public class PrepareGameplaySceneState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameplaySceneState(IGameStateMachine gameStateMachine,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _gameStateMachine = gameStateMachine;
            _prefabsListConfig = gameplayConfigsProvider.GetPrefabsListConfig();

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly PrefabsListConfigMeta _prefabsListConfig;

        private NetworkManager _networkManager;
        private ulong _clientID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateNetworkPrefabs();
            EnterGameplaySceneState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateNetworkPrefabs()
        {
            _networkManager = NetworkManager.Singleton;
            
            if (!_networkManager.IsHost)
                return;
            
            _clientID = _networkManager.LocalClientId;

            CreateGameManager();
            CreateRpcCaller();
            CreatePlayerSpawner();
            CreateElevatorsManager();
            CreateQuestsManager();
            CreateDeliveryManager();
            CreateGameTimeManager();
        }

        private void CreateGameManager()
        {
            GameManager gameManagerPrefab = _prefabsListConfig.GameManager;
            CreateNetworkPrefab(gameManagerPrefab.gameObject);
        }

        private void CreateRpcCaller()
        {
            RpcHandler rpcHandlerPrefab = _prefabsListConfig.RpcHandler;
            CreateNetworkPrefab(rpcHandlerPrefab.gameObject);
        }

        private void CreatePlayerSpawner()
        {
            PlayerSpawner playerSpawnerPrefab = _prefabsListConfig.PlayerSpawner;
            CreateNetworkPrefab(playerSpawnerPrefab.gameObject);
        }

        private void CreateElevatorsManager()
        {
            ElevatorsManager elevatorsManagerPrefab = _prefabsListConfig.ElevatorsManager;
            CreateNetworkPrefab(elevatorsManagerPrefab.gameObject);
        }

        private void CreateQuestsManager()
        {
            QuestsManager questsManagerPrefab = _prefabsListConfig.QuestsManager;
            CreateNetworkPrefab(questsManagerPrefab.gameObject);
        }

        private void CreateDeliveryManager()
        {
            DeliveryManager deliveryManagerPrefab = _prefabsListConfig.DeliveryManager;
            CreateNetworkPrefab(deliveryManagerPrefab.gameObject);
        }

        private void CreateGameTimeManager()
        {
            GameTimeManager gameTimeManagerPrefab = _prefabsListConfig.GameTimeManager;
            CreateNetworkPrefab(gameTimeManagerPrefab.gameObject);
        }

        private void CreateNetworkPrefab(GameObject gameObject)
        {
            var networkObjectPrefab = gameObject.GetComponent<NetworkObject>();
            _networkManager.SpawnManager.InstantiateAndSpawn(networkObjectPrefab, _clientID, destroyWithScene: true);
        }

        private void EnterGameplaySceneState() =>
            _gameStateMachine.ChangeState<GameplaySceneState>();
    }
}