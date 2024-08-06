using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Game;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameManagerDecorator gameManagerDecorator,
            IHorrorStateMachine horrorStateMachine,
            INetworkHorror networkHorror,
            IGameObserver gameObserver,
            ILevelObserver levelObserver,
            IDungeonsObserver dungeonsObserver,
            ITrainEntity trainEntity,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _gameManagerDecorator = gameManagerDecorator;
            _horrorStateMachine = horrorStateMachine;
            _networkHorror = networkHorror;
            _gameObserver = gameObserver;
            _levelObserver = levelObserver;
            _dungeonsObserver = dungeonsObserver;
            _trainEntity = trainEntity;
            _networkSceneManager = NetworkManager.Singleton.SceneManager;
            _balanceConfig = gameplayConfigsProvider.GetBalanceConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const LocationName DefaultCurrentLocation = LocationName.Base;
        private const LocationName DefaultSelectedLocation = LocationName.Forest;
        private const GameState DefaultGameState = GameState.Gameplay;

        private readonly NetworkVariable<LocationName> _currentLocation = new(DefaultCurrentLocation);
        private readonly NetworkVariable<LocationName> _selectedLocation = new(DefaultSelectedLocation);
        private readonly NetworkVariable<GameState> _gameState = new(DefaultGameState);
        private readonly NetworkVariable<int> _playersGold = new();

        private readonly Dictionary<ulong, GameState> _playersStates = new(capacity: 8); // Server only.
        private readonly Dictionary<ulong, bool> _playersLoadingStates = new(capacity: 8); // Server only.

        private IGameManagerDecorator _gameManagerDecorator;
        private IHorrorStateMachine _horrorStateMachine;
        private INetworkHorror _networkHorror;
        private IGameObserver _gameObserver;
        private ILevelObserver _levelObserver;
        private IDungeonsObserver _dungeonsObserver;
        private ITrainEntity _trainEntity;
        private NetworkSceneManager _networkSceneManager;
        private BalanceConfigMeta _balanceConfig;

        private LocationName _previousSelectedLocation = DefaultSelectedLocation;
        private bool _isScenesSynchronized;
        private bool _isServerLocationLoaded;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameManagerDecorator.OnChangeGameStateInnerEvent += ChangeGameState;
            _gameManagerDecorator.OnChangeGameStateWhenAllPlayersReadyInnerEvent += ChangeGameStateWhenAllPlayersReady;
            _gameManagerDecorator.OnStartGameRestartTimerInnerEvent += StartGameRestartTimer;
            _gameManagerDecorator.OnSelectLocationInnerEvent += SelectLocation;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent += LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent += AddPlayersGold;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent += SpendPlayersGold;
            _gameManagerDecorator.OnResetPlayersGoldInnerEvent += ResetGold;
            _gameManagerDecorator.OnGetCurrentLocationInnerEvent += GetCurrentLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent += GetSelectedLocation;
            _gameManagerDecorator.OnGetPreviousLocationInnerEvent += GetPreviousLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent += GetGameState;

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
        }

        private void Start()
        {
            if (!IsServerOnly)
                return;

            _gameObserver.TrainArrivedAtBase(LocationName.Base);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _gameManagerDecorator.OnChangeGameStateInnerEvent -= ChangeGameState;
            _gameManagerDecorator.OnChangeGameStateWhenAllPlayersReadyInnerEvent -= ChangeGameStateWhenAllPlayersReady;
            _gameManagerDecorator.OnStartGameRestartTimerInnerEvent -= StartGameRestartTimer;
            _gameManagerDecorator.OnSelectLocationInnerEvent -= SelectLocation;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent -= LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent -= AddPlayersGold;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent -= SpendPlayersGold;
            _gameManagerDecorator.OnResetPlayersGoldInnerEvent -= ResetGold;
            _gameManagerDecorator.OnGetCurrentLocationInnerEvent -= GetCurrentLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent -= GetSelectedLocation;
            _gameManagerDecorator.OnGetPreviousLocationInnerEvent -= GetPreviousLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent -= GetGameState;

            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _currentLocation.OnValueChanged += OnCurrentLocationChanged;

            _selectedLocation.OnValueChanged += OnSelectedLocationChanged;

            _gameState.OnValueChanged += OnAllGameStateChanged;

            _playersGold.OnValueChanged += OnPlayersGoldChanged;
        }

        protected override void InitServerOnly()
        {
            _networkHorror.OnPlayerConnectedEvent += OnPlayerConnected;
            _networkHorror.OnPlayerDisconnectedEvent += OnPlayerDisconnected;

            _levelObserver.OnLocationLoadedEvent += OnServerLocationLoaded;
            _levelObserver.OnLocationUnloadedEvent += OnServerLocationUnloaded;

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent += OnDungeonsGenerationCompleted;

            _networkSceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
            _networkSceneManager.OnUnloadEventCompleted += OnSceneUnloadCompleted;

            _trainEntity.OnMovementStoppedEvent += OnTrainMovementStopped;
            _trainEntity.OnMovementStartedEvent += OnTrainMovementStarted;
        }

        protected override void DespawnAll()
        {
            _currentLocation.OnValueChanged -= OnCurrentLocationChanged;

            _selectedLocation.OnValueChanged -= OnSelectedLocationChanged;

            _gameState.OnValueChanged -= OnAllGameStateChanged;

            _playersGold.OnValueChanged -= OnPlayersGoldChanged;
        }

        protected override void DespawnServerOnly()
        {
            _networkHorror.OnPlayerConnectedEvent -= OnPlayerConnected;
            _networkHorror.OnPlayerDisconnectedEvent -= OnPlayerDisconnected;

            _levelObserver.OnLocationLoadedEvent -= OnServerLocationLoaded;
            _levelObserver.OnLocationUnloadedEvent -= OnServerLocationUnloaded;

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent -= OnDungeonsGenerationCompleted;

            if (_networkSceneManager != null)
            {
                _networkSceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
                _networkSceneManager.OnUnloadEventCompleted -= OnSceneUnloadCompleted;
            }

            _trainEntity.OnMovementStoppedEvent -= OnTrainMovementStopped;
            _trainEntity.OnMovementStartedEvent -= OnTrainMovementStarted;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeGameState(GameState gameState)
        {
            // Debug.LogWarning("----> GAME STATE changed!!! " + gameState);

            if (IsServerOnly)
                _gameState.Value = gameState;
            else
                ChangeGameStateServerRpc(gameState);
        }

        private void SelectLocation(LocationName locationName)
        {
            if (IsServerOnly)
                _selectedLocation.Value = locationName;
            else
                SelectLocationServerRpc(locationName);
        }

        private void AddPlayersGold(int amount)
        {
            if (IsServerOnly)
                _playersGold.Value += amount;
            else
                AddPlayersGoldServerRpc(amount);
        }

        private void SpendPlayersGold(int amount)
        {
            if (IsServerOnly)
                _playersGold.Value -= amount;
            else
                SpendPlayersGoldServerRpc(amount);
        }

        private void ResetGold()
        {
            if (IsServerOnly)
                _playersGold.Value = 0;
            else
                ResetGoldServerRpc();
        }

        private void CheckGameOverOnServer()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            int deadPlayersAmount = 0;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (!isDead)
                    continue;

                deadPlayersAmount++;
            }

            int totalPlayersAmount = allPlayers.Count;
            bool isGameOver = totalPlayersAmount == deadPlayersAmount;

            string log = Log.HandleLog($"Is Game Over: <gb>{isGameOver}</gb>.");
            Debug.Log(log);

            if (!isGameOver)
                return;

            ChangeGameState(GameState.GameOver);
        }

        private async void ChangeGameStateWhenAllPlayersReady(GameState newState, GameState previousState)
        {
            const int checkDelay = 100;
            const int maxIterations = 100; // При 100 мс = 10 секунд проверка.
            bool playersStatesAreSynchronized = false;

            for (int i = 0; i < maxIterations; i++)
            {
                bool isCanceled = await UniTask
                    .Delay(checkDelay, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    break;

                bool isSynchronized = true;

                foreach (GameState playersState in _playersStates.Values)
                {
                    bool isMatches = playersState == previousState;

                    if (isMatches)
                        continue;

                    isSynchronized = false;
                    break;
                }

                if (!isSynchronized)
                    continue;

                playersStatesAreSynchronized = true;
                break;
            }

            if (!playersStatesAreSynchronized)
                Log.PrintError(log: "Players States was not synchronized!");

            ChangeGameState(newState);
        }

        private void LocationLoadedLogic()
        {
            bool isLocationLoaded = _isServerLocationLoaded && _isScenesSynchronized;

            if (!isLocationLoaded)
                return;

            _gameObserver.TrainLeavingBase();
        }

        private void LocationUnloadedLogic()
        {
            bool isLocationUnloaded = !_isServerLocationLoaded && !_isScenesSynchronized;

            if (!isLocationUnloaded)
                return;
        }

        private void StartGameRestartTimer()
        {
            IEnumerator routine = RestartGameTimerCO();
            StartCoroutine(routine);
        }

        private IEnumerator RestartGameTimerCO()
        {
            float delay = _balanceConfig.GameRestartDelay;
            yield return new WaitForSeconds(delay);

            GameState previousState = _gameState.Value;
            ChangeGameStateWhenAllPlayersReady(newState: GameState.RestartGame, previousState);
        }

        private LocationName GetCurrentLocation() =>
            _currentLocation.Value;

        private LocationName GetSelectedLocation() =>
            _selectedLocation.Value;

        private LocationName GetPreviousLocation() => _previousSelectedLocation;

        private GameState GetGameState() =>
            _gameState.Value;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void ChangeGameStateServerRpc(GameState gameState) =>
            _gameState.Value = gameState;

        [ServerRpc(RequireOwnership = false)]
        private void SelectLocationServerRpc(LocationName locationName) =>
            _selectedLocation.Value = locationName;

        [ServerRpc(RequireOwnership = false)]
        private void LoadSelectedLocationServerRpc()
        {
            LocationName selectedLocation = _selectedLocation.Value;
            _horrorStateMachine.ChangeState<LoadLocationState, LocationName>(selectedLocation);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddPlayersGoldServerRpc(int amount) =>
            _playersGold.Value += amount;

        [ServerRpc(RequireOwnership = false)]
        private void SpendPlayersGoldServerRpc(int amount) =>
            _playersGold.Value -= amount;

        [ServerRpc(RequireOwnership = false)]
        private void ResetGoldServerRpc() =>
            _playersGold.Value = 0;

        [ServerRpc(RequireOwnership = false)]
        private void SyncPlayerGameStateServerRpc(GameState gameState, ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;
            bool containsPlayer = _playersStates.ContainsKey(clientID);

            if (!containsPlayer)
            {
                Log.PrintError(log: $"Players States <rb>doesn't contains player</rb> with ID <gb>({clientID})</gb>!");
                return;
            }

            _playersStates[clientID] = gameState;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerConnected(ulong clientID)
        {
            _playersStates.TryAdd(clientID, _gameState.Value);
            _playersLoadingStates.TryAdd(clientID, false);
        }

        private void OnPlayerDisconnected(ulong clientID)
        {
            _playersStates.Remove(clientID);
            _playersLoadingStates.Remove(clientID);
        }

        private void OnCurrentLocationChanged(LocationName previousValue, LocationName newValue) =>
            _gameManagerDecorator.CurrentLocationChanged(newValue);

        private void OnSelectedLocationChanged(LocationName previousValue, LocationName newValue) =>
            _gameManagerDecorator.SelectedLocationChanged(newValue);

        private void OnAllGameStateChanged(GameState previousValue, GameState newValue)
        {
            string log = Log.HandleLog(tag: "Game State", log: $"<gb>{previousValue}</gb> ---> <gb>{newValue}</gb>");
            Debug.Log(log);

            if (IsServerOnly)
                _playersStates[OwnerClientId] = newValue;
            else
                SyncPlayerGameStateServerRpc(newValue);

            _gameManagerDecorator.GameStateChanged(gameState: newValue);
        }

        private void OnPlayersGoldChanged(int previousValue, int newValue) =>
            _gameManagerDecorator.PlayersGoldChanged(playersGold: newValue);

        private void OnServerLocationLoaded()
        {
            LocationName selectedLocation = GetSelectedLocation();
            _currentLocation.Value = selectedLocation;
            _previousSelectedLocation = selectedLocation;
            _isServerLocationLoaded = true;

            LocationLoadedLogic();
        }

        private void OnServerLocationUnloaded()
        {
            _currentLocation.Value = _selectedLocation.Value;
            _selectedLocation.Value = _previousSelectedLocation;
            _isServerLocationLoaded = false;

            LocationUnloadedLogic();
        }

#warning ЗАМЕНИТЬ НА СИНХРОНИЗИРУЕМУЮ У ВСЕХ ВЕРСИЮ
        private void OnDungeonsGenerationCompleted() =>
            _gameObserver.TrainArrivedAtSector();

        private void OnTrainMovementStopped() =>
            _gameObserver.TrainStoppedAtSector();

        private void OnTrainMovementStarted()
        {
            LocationName currentLocation = GetCurrentLocation();
            bool isLeavingBase = currentLocation == LocationName.Base;

            if (!isLeavingBase)
                _gameObserver.TrainLeavingSector();
        }

        private void OnSceneLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            string log = Log.HandleLog(
                $"Scene <gb>{sceneName}</gb> loaded,   " +
                $"mode: <gb>{loadSceneMode}</gb>,   " +
                $"clients: {clientsCompleted.Count}"
            );

            Debug.Log(log);

            if (loadSceneMode != LoadSceneMode.Additive)
                return;

            _isScenesSynchronized = true;
            LocationLoadedLogic();
        }

        private void OnSceneUnloadCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            string log = Log.HandleLog(
                $"Scene <gb>{sceneName}</gb> unloaded,   " +
                $"mode: <gb>{loadSceneMode}</gb>,   " +
                $"clients: {clientsCompleted.Count}"
            );

            Debug.Log(log);

            _isScenesSynchronized = false;
            LocationUnloadedLogic();
        }

        private void OnPlayerSpawned(PlayerEntity playerEntity) =>
            playerEntity.OnDiedEvent += OnPlayerDied;

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            if (!playerEntity.IsServerOnly)
                CheckGameOverOnServer();

            playerEntity.OnDiedEvent -= OnPlayerDied;
        }

        private void OnPlayerDied() => CheckGameOverOnServer();

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugAddPlayersGold(int amount) => AddPlayersGold(amount);

        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugSpendPlayersGold(int amount) => SpendPlayersGold(amount);
    }
}