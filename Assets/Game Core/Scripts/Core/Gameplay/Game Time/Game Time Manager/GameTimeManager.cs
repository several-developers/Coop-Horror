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
        private void Construct(IGameManagerDecorator gameManagerDecorator, ITimeCycle timeCycle)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _timeCycle = timeCycle;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeCycle _timeCycle;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServer()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        protected override void InitClient() =>
            _gameTimer.OnValueChanged += OnGameTimerUpdated;

        protected override void TickServerAndClient() =>
            _timeCycle.Tick(); // Check for optimization

        protected override void DespawnServer()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

            _timeCycle.OnHourPassedEvent -= OnHourPassed;
        }

        protected override void DespawnClient() =>
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