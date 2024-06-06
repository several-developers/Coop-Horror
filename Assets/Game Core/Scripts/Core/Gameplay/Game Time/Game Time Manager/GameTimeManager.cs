using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameTimeManagerDecorator gameTimeManagerDecorator,
            IGameManagerDecorator gameManagerDecorator, ITimeCycle timeCycle)
        {
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;
            _timeCycle = timeCycle;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private IGameTimeManagerDecorator _gameTimeManagerDecorator;
        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeCycle _timeCycle;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameTimeManagerDecorator.OnResetDayInnerEvent += ResetDay;
            _gameTimeManagerDecorator.OnSetMidnightInnerEvent += SetMidnight;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _gameTimeManagerDecorator.OnResetDayInnerEvent -= ResetDay;
            _gameTimeManagerDecorator.OnSetMidnightInnerEvent -= SetMidnight;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitOwner()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        protected override void InitNotOwner() =>
            _gameTimer.OnValueChanged += OnGameTimerUpdated;

        protected override void TickAll() =>
            _timeCycle.Tick(); // Check for optimization

        protected override void DespawnOwner()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

            _timeCycle.OnHourPassedEvent -= OnHourPassed;
        }

        protected override void DespawnNotOwner() =>
            _gameTimer.OnValueChanged -= OnGameTimerUpdated;

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
                    SetMidnight();
                    break;
            }
        }

        private void ResetDay()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            dateTime.ResetDay();
            _timeCycle.SyncDateTime(dateTime);
        }

        private void SetMidnight() =>
            _timeCycle.SetMidnight();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

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