using System;
using GameCore.Gameplay.Levels.GameTime;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using Zenject;

namespace GameCore.Core.Gameplay.GameTimerManagement
{
    public class GameTimerManager : NetworkBehaviour, INetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycleDecorator timeCycleDecorator) =>
            _timeCycleDecorator = timeCycleDecorator;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private ITimeCycleDecorator _timeCycleDecorator;

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
            
            _timeCycleDecorator.OnHourPassedEvent += OnHourPassed;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
            
            _gameTimer.OnValueChanged += OnGameTimerUpdated;
        }

        public void TickServerAndClient() =>
            _timeCycleDecorator.Tick();

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
            
            _timeCycleDecorator.OnHourPassedEvent -= OnHourPassed;
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
            DateTime dateTime = _timeCycleDecorator.GetDateTime();
            MyDateTime myDateTime = new(dateTime.Second, dateTime.Minute, dateTime.Hour);
            _gameTimer.Value = myDateTime;
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

        private void OnHourPassed() => UpdateGameTimer();

        private void OnGameTimerUpdated(MyDateTime previousDate, MyDateTime newDate) =>
            _timeCycleDecorator.SyncDateTime(newDate);
    }
}