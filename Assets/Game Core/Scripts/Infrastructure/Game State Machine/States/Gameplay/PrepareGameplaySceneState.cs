using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Gameplay.ChatManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Infrastructure.StateMachine
{
    public class PrepareGameplaySceneState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameplaySceneState(
            IGameStateMachine gameStateMachine,
            IAssetsProvider assetsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _gameStateMachine = gameStateMachine;
            _assetsProvider = assetsProvider;
            _prefabsListConfig = gameplayConfigsProvider.GetPrefabsListConfig();

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAssetsProvider _assetsProvider;
        private readonly PrefabsListConfigMeta _prefabsListConfig;

        private NetworkManager _networkManager;
        private ulong _clientID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CleanUpAddressables();
            CreateNetworkPrefabs();
            EnterGameplaySceneState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CleanUpAddressables() =>
            _assetsProvider.Cleanup();

        private void CreateNetworkPrefabs()
        {
            _networkManager = NetworkManager.Singleton;

            if (!_networkManager.IsHost)
                return;

            _clientID = _networkManager.LocalClientId;

            CreateGameManager();
            CreatePlayerSpawner();
            CreateElevatorsManager();
            CreateQuestsManager();
            CreateGameTimeManager();
            CreateChatManager();
            CreateNoiseManager();
            //CreateGameObserver();
        }

        private void CreateGameManager()
        {
            GameManager gameManagerPrefab = _prefabsListConfig.GameManager;
            CreateNetworkPrefab(gameManagerPrefab.gameObject);
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

        private void CreateGameTimeManager()
        {
            GameTimeManager gameTimeManagerPrefab = _prefabsListConfig.GameTimeManager;
            CreateNetworkPrefab(gameTimeManagerPrefab.gameObject);
        }

        private void CreateChatManager()
        {
            ChatManager chatManagerPrefab = _prefabsListConfig.ChatManager;
            CreateNetworkPrefab(chatManagerPrefab.gameObject);
        }

        private void CreateNoiseManager()
        {
            NoiseManager noiseManagerPrefab = _prefabsListConfig.NoiseManager;
            CreateNetworkPrefab(noiseManagerPrefab.gameObject);
        }

        // private void CreateGameObserver()
        // {
        //     GameObserver gameObserverPrefab = _prefabsListConfig.GameObserver;
        //     CreateNetworkPrefab(gameObserverPrefab.gameObject);
        // }

        private void CreateNetworkPrefab(GameObject gameObject)
        {
            var networkObjectPrefab = gameObject.GetComponent<NetworkObject>();
            _networkManager.SpawnManager.InstantiateAndSpawn(networkObjectPrefab, _clientID, destroyWithScene: true);
        }

        private void EnterGameplaySceneState() =>
            _gameStateMachine.ChangeState<GameplaySceneState>();
    }
}