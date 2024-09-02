using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.Time;
using Sirenix.OdinInspector;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameTimeManagerDecorator gameTimeManagerDecorator,
            ITimeCycle timeCycle,
            ITimeObserver timeObserver
        )
        {
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _timeCycle = timeCycle;
            _timeObserver = timeObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<MyDateTime> _gameTimer = new();

        private IGameTimeManagerDecorator _gameTimeManagerDecorator;
        private ITimeCycle _timeCycle;
        private ITimeObserver _timeObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameTimeManagerDecorator.OnSetSunriseInnerEvent += SetSunrise;
            _gameTimeManagerDecorator.OnSetMidnightInnerEvent += SetMidnight;
            _gameTimeManagerDecorator.OnIncreaseDayInnerEvent += IncreaseDay;
            _gameTimeManagerDecorator.OnResetDayInnerEvent += ResetDay;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _gameTimeManagerDecorator.OnSetSunriseInnerEvent -= SetSunrise;
            _gameTimeManagerDecorator.OnSetMidnightInnerEvent -= SetMidnight;
            _gameTimeManagerDecorator.OnIncreaseDayInnerEvent -= IncreaseDay;
            _gameTimeManagerDecorator.OnResetDayInnerEvent -= ResetDay;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitOwner() =>
            _timeObserver.OnMinutePassedEvent += OnMinutePassed;

        protected override void InitNotOwner() =>
            _gameTimer.OnValueChanged += OnGameTimerUpdated;

        protected override void TickAll() =>
            _timeCycle.Tick(); // Check for optimization

        protected override void DespawnOwner() =>
            _timeObserver.OnMinutePassedEvent -= OnMinutePassed;

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

        private void SetSunrise() =>
            _timeCycle.SetSunrise();

        private void SetMidnight() =>
            _timeCycle.SetMidnight();

        private void ResetDay()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            dateTime.ResetDay();
            _timeCycle.SyncDateTime(dateTime);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMinutePassed() => UpdateGameTimer();

        private void OnGameTimerUpdated(MyDateTime previousDate, MyDateTime newDate) =>
            _timeCycle.SyncDateTime(newDate);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugIncreaseDay() => IncreaseDay();
    }
}