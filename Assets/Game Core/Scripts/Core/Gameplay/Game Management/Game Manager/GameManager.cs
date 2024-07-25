using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Game;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
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
            ICamerasManager camerasManager,
            IGameObserver gameObserver,
            ILevelObserver levelObserver,
            ITrainEntity trainEntity,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _gameManagerDecorator = gameManagerDecorator;
            _horrorStateMachine = horrorStateMachine;
            _networkHorror = networkHorror;
            _camerasManager = camerasManager;
            _gameObserver = gameObserver;
            _levelObserver = levelObserver;
            _trainEntity = trainEntity;
            _balanceConfig = gameplayConfigsProvider.GetBalanceConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const LocationName DefaultCurrentLocation = LocationName.Base;
        private const LocationName DefaultSelectedLocation = LocationName.Scrapyard;
        private const GameState DefaultGameState = GameState.Gameplay;

        private readonly NetworkVariable<LocationName> _currentLocation = new(DefaultCurrentLocation);
        private readonly NetworkVariable<LocationName> _selectedLocation = new(DefaultSelectedLocation);
        private readonly NetworkVariable<GameState> _gameState = new(DefaultGameState);
        private readonly NetworkVariable<int> _playersGold = new();

        private readonly Dictionary<ulong, GameState> _playersStates = new(capacity: 8); // Server only.

        private IGameManagerDecorator _gameManagerDecorator;
        private IHorrorStateMachine _horrorStateMachine;
        private INetworkHorror _networkHorror;
        private ICamerasManager _camerasManager;
        private IGameObserver _gameObserver;
        private ILevelObserver _levelObserver;
        private ITrainEntity _trainEntity;
        private BalanceConfigMeta _balanceConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameManagerDecorator.OnChangeGameStateInnerEvent += ChangeGameState;
            _gameManagerDecorator.OnChangeGameStateWhenAllPlayersReadyInnerEvent += ChangeGameStateWhenAllPlayersReady;
            _gameManagerDecorator.OnSelectLocationInnerEvent += SelectLocation;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent += LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent += AddPlayersGold;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent += SpendPlayersGold;
            _gameManagerDecorator.OnResetPlayersGoldInnerEvent += ResetGold;
            _gameManagerDecorator.OnGetCurrentLocationInnerEvent += GetCurrentLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent += GetSelectedLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent += GetGameState;

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _gameManagerDecorator.OnChangeGameStateInnerEvent -= ChangeGameState;
            _gameManagerDecorator.OnChangeGameStateWhenAllPlayersReadyInnerEvent -= ChangeGameStateWhenAllPlayersReady;
            _gameManagerDecorator.OnSelectLocationInnerEvent -= SelectLocation;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent -= LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent -= AddPlayersGold;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent -= SpendPlayersGold;
            _gameManagerDecorator.OnResetPlayersGoldInnerEvent -= ResetGold;
            _gameManagerDecorator.OnGetCurrentLocationInnerEvent -= GetCurrentLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent -= GetSelectedLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent -= GetGameState;

            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _selectedLocation.OnValueChanged += OnSelectedLocationChanged;
            _gameState.OnValueChanged += OnAllGameStateChanged;
            _playersGold.OnValueChanged += OnPlayersGoldChanged;
        }

        protected override void InitServerOnly()
        {
            _networkHorror.OnPlayerConnectedEvent += OnPlayerConnected;
            _networkHorror.OnPlayerDisconnectedEvent += OnPlayerDisconnected;

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;

            _trainEntity.OnMovementStoppedEvent += OnTrainMovementStopped;
            _trainEntity.OnMovementStartedEvent += OnTrainMovementStarted;
        }

        protected override void DespawnAll()
        {
            _selectedLocation.OnValueChanged -= OnSelectedLocationChanged;
            _gameState.OnValueChanged -= OnAllGameStateChanged;
            _playersGold.OnValueChanged -= OnPlayersGoldChanged;
        }

        protected override void DespawnServerOnly()
        {
            _networkHorror.OnPlayerConnectedEvent -= OnPlayerConnected;
            _networkHorror.OnPlayerDisconnectedEvent -= OnPlayerDisconnected;

            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;
            
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

        private void HandleGameState(GameState gameState)
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();

            switch (gameState)
            {
                /*case GameState.GameOver:
                    _camerasManager.SetCameraStatus(CameraStatus.OutsideMobileHQ);

                    if (!IsServerOnly)
                        return;

                    StartCoroutine(routine: RestartGameTimerCO());
                    break;

                case GameState.HeadingToTheRoad:
                    localPlayer.SetEntityLocation(EntityLocation.Road);
                    break;

                case GameState.HeadingToTheLocation:
                    localPlayer.SetEntityLocation(EntityLocation.LocationSurface);
                    break;

                case GameState.ArrivedAtTheRoad:
                    if (!IsServerOnly)
                        return;

                    ChangeGameStateWhenAllPlayersReady(newState: GameState.EnteringMainRoad,
                        previousState: GameState.ArrivedAtTheRoad);
                    break;

                case GameState.QuestsRewarding:
                    if (!IsServerOnly)
                        return;

                    ChangeGameStateWhenAllPlayersReady(newState: GameState.CycleMovement,
                        previousState: GameState.QuestsRewarding);
                    break;

                case GameState.ArrivedAtTheLocation:
                    localPlayer.EnterAliveState();

                    if (!IsServerOnly)
                        return;

                    IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();

                    foreach (PlayerEntity playerEntity in allPlayers.Values)
                        playerEntity.NetworkObject.TryRemoveParent();

                    ChangeGameStateWhenAllPlayersReady(newState: GameState.ReadyToLeaveTheLocation,
                        previousState: GameState.ArrivedAtTheLocation);
                    break;

                case GameState.RestartGame:
                    ChangeGameStateWhenAllPlayersReady(newState: GameState.RestartGameCompleted,
                        previousState: GameState.RestartGame);
                    break;

                case GameState.RestartGameCompleted:
                    ChangeGameStateWhenAllPlayersReady(newState: GameState.CycleMovement,
                        previousState: GameState.RestartGameCompleted);
                    break;*/
            }
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

        private void OnPlayerConnected(ulong clientID) =>
            _playersStates.TryAdd(clientID, _gameState.Value);

        private void OnPlayerDisconnected(ulong clientID) =>
            _playersStates.Remove(clientID);

        private void OnSelectedLocationChanged(LocationName previousValue, LocationName newValue) =>
            _gameManagerDecorator.SelectedLocationChanged(newValue);

        private void OnAllGameStateChanged(GameState previousValue, GameState newValue)
        {
            string log = Log.HandleLog("Game State", $"<gb>{previousValue}</gb> ---> <gb>{newValue}</gb>");
            Debug.Log(log);

            if (IsServerOnly)
                _playersStates[OwnerClientId] = newValue;
            else
                SyncPlayerGameStateServerRpc(newValue);

            _gameManagerDecorator.GameStateChanged(gameState: newValue);
            HandleGameState(gameState: newValue);
        }

        private void OnPlayersGoldChanged(int previousValue, int newValue) =>
            _gameManagerDecorator.PlayersGoldChanged(playersGold: newValue);

        private void OnLocationLoaded()
        {
            _currentLocation.Value = _selectedLocation.Value;
            
            LocationName currentLocation = GetCurrentLocation();
            bool arrivedAtBase = currentLocation == LocationName.Base;

            if (arrivedAtBase)
                _gameObserver.TrainArrivedAtBase();
            else
                _gameObserver.TrainArrivedAtSector();
        }

        private void OnTrainMovementStopped() =>
            _gameObserver.TrainStoppedAtSector();

        private void OnTrainMovementStarted()
        {
            LocationName currentLocation = GetCurrentLocation();
            bool isLeavingBase = currentLocation == LocationName.Base;
            
            if (isLeavingBase)
                _gameObserver.TrainLeavingBase();
            else
                _gameObserver.TrainLeavingSector();
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