using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManager : NetworkBehaviour, INetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator, ITimeCycle timeCycle)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _timeCycle = timeCycle;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeCycle _timeCycle;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            TickServerAndClient();
            TickServer();
            TickClient();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            // TO DO
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;

            _gameTimer.OnValueChanged += OnGameTimerUpdated;
        }

        public void TickServerAndClient() =>
            _timeCycle.Tick(); // Check for optimization

        public void TickServer()
        {
            if (!IsOwner)
                return;

            // TO DO
        }

        public void TickClient()
        {
            if (IsOwner)
                return;

            // TO DO
        }

        public void DespawnServerAndClient()
        {
            // TO DO
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _timeCycle.OnHourPassedEvent -= OnHourPassed;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;

            _gameTimer.OnValueChanged -= OnGameTimerUpdated;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateGameTimer()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            _gameTimer.Value = dateTime;
        }

        private void IncreaseDay()
        {
            _timeCycle.IncreaseDay();
            UpdateGameTimer();
        }

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.HeadingToTheLocation:
                    _timeCycle.SetSunrise();
                    IncreaseDay();
                    break;
                
                case GameState.ArrivedAtTheRoad:
                    _timeCycle.SetMidnight();
                    break;
                
                case GameState.RestartGame:
                    MyDateTime dateTime = _timeCycle.GetDateTime();
                    dateTime.ResetDay();
                    _timeCycle.SyncDateTime(dateTime);
                    break;
            }
        }

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
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();

            base.OnNetworkDespawn();
        }

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnHourPassed() => UpdateGameTimer();

        private void OnGameTimerUpdated(MyDateTime previousDate, MyDateTime newDate) =>
            _timeCycle.SyncDateTime(newDate);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugIncreaseDay() => IncreaseDay();
    }
}