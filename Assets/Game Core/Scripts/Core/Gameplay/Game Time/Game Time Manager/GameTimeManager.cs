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
        private void Construct(IGameTimeManagerDecorator gameTimeManagerDecorator, ITimeCycle timeCycle)
        {
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _timeCycle = timeCycle;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private IGameTimeManagerDecorator _gameTimeManagerDecorator;
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

            _gameTimeManagerDecorator.OnIncreaseDayEvent += OnIncreaseDay;
            
            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
            
            _gameTimer.OnValueChanged += OnGameTimerUpdated;
        }

        public void TickServerAndClient() =>
            _timeCycle.Tick();

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
            
            _gameTimeManagerDecorator.OnIncreaseDayEvent -= OnIncreaseDay;

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

        private void OnIncreaseDay() => IncreaseDay();

        private void OnHourPassed() => UpdateGameTimer();

        private void OnGameTimerUpdated(MyDateTime previousDate, MyDateTime newDate) =>
            _timeCycle.SyncDateTime(newDate);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugIncreaseDay() => IncreaseDay();
    }
}