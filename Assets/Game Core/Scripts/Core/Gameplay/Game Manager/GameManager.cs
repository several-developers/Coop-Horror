using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManager : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator, IHorrorStateMachine horrorStateMachine,
            ILevelObserver levelObserver)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _horrorStateMachine = horrorStateMachine;
            _levelObserver = levelObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const SceneName DefaultLocation = SceneName.Desert;
        private const GameState DefaultGameState = GameState.ReadyToLeaveTheRoad;

        private readonly NetworkVariable<SceneName> _selectedLocation = new(DefaultLocation);
        private readonly NetworkVariable<GameState> _gameState = new(DefaultGameState);
        private readonly NetworkVariable<int> _playersGold = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private IHorrorStateMachine _horrorStateMachine;
        private ILevelObserver _levelObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameManagerDecorator.OnChangeGameStateInnerEvent += ChangeGameStateServerRpc;
            _gameManagerDecorator.OnSelectLocationInnerEvent += SelectLocationServerRpc;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent += LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent += AddPlayersGoldServerRpc;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent += SpendPlayersGoldServerRpc;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent += GetSelectedLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent += GetGameState;
        }

        public override void OnDestroy()
        {
            _gameManagerDecorator.OnChangeGameStateInnerEvent -= ChangeGameStateServerRpc;
            _gameManagerDecorator.OnSelectLocationInnerEvent -= SelectLocationServerRpc;
            _gameManagerDecorator.OnLoadSelectedLocationInnerEvent -= LoadSelectedLocationServerRpc;
            _gameManagerDecorator.OnAddPlayersGoldInnerEvent -= AddPlayersGoldServerRpc;
            _gameManagerDecorator.OnSpendPlayersGoldInnerEvent -= SpendPlayersGoldServerRpc;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent -= GetSelectedLocation;
            _gameManagerDecorator.OnGetGameStateInnerEvent -= GetGameState;

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _selectedLocation.OnValueChanged += OnSelectedLocationChanged;
            _gameState.OnValueChanged += OnGameStateChanged;
            _playersGold.OnValueChanged += OnPlayersGoldChanged;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;
            _levelObserver.OnLocationLeftEvent += OnLocationLeft;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient()
        {
            _selectedLocation.OnValueChanged -= OnSelectedLocationChanged;
            _gameState.OnValueChanged -= OnGameStateChanged;
            _playersGold.OnValueChanged -= OnPlayersGoldChanged;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;
            _levelObserver.OnLocationLeftEvent -= OnLocationLeft;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private SceneName GetSelectedLocation() =>
            _selectedLocation.Value;

        private GameState GetGameState() =>
            _gameState.Value;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void ChangeGameStateServerRpc(GameState gameState) =>
            _gameState.Value = gameState;

        [ServerRpc(RequireOwnership = false)]
        private void SelectLocationServerRpc(SceneName sceneName) =>
            _selectedLocation.Value = sceneName;

        [ServerRpc(RequireOwnership = false)]
        private void LoadSelectedLocationServerRpc()
        {
            SceneName selectedLocation = _selectedLocation.Value;
            _horrorStateMachine.ChangeState<LoadLocationState, SceneName>(selectedLocation);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddPlayersGoldServerRpc(int amount) =>
            _playersGold.Value += amount;

        [ServerRpc(RequireOwnership = false)]
        private void SpendPlayersGoldServerRpc(int amount) =>
            _playersGold.Value -= amount;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void OnSelectedLocationChanged(SceneName previousValue, SceneName newValue) =>
            _gameManagerDecorator.SelectedLocationChanged(newValue);

        private void OnGameStateChanged(GameState previousValue, GameState newValue)
        {
            string log = Log.HandleLog("Game State", $"<gb>{previousValue}</gb> ---> <gb>{newValue}</gb>");
            Debug.Log(log);

            _gameManagerDecorator.GameStateChanged(gameState: newValue);
        }

        private void OnPlayersGoldChanged(int previousValue, int newValue) =>
            _gameManagerDecorator.PlayersGoldChanged(playersGold: newValue);

        private void OnLocationLoaded() => ChangeGameStateServerRpc(GameState.HeadingToTheLocation);

        private void OnLocationLeft() => ChangeGameStateServerRpc(GameState.ArrivedAtTheRoad);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugAddPlayersGold(int amount) => AddPlayersGoldServerRpc(amount);

        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugSpendPlayersGold(int amount) => SpendPlayersGoldServerRpc(amount);
    }
}