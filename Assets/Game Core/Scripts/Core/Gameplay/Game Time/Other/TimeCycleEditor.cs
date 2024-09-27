using GameCore.Observers.Gameplay.Time;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class TimeCycleEditor : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeService timeService, ITimeObserver timeObserver)
        {
            _timeService = timeService;
            _timeObserver = timeObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(CycleInfo)]
        [SerializeField, ReadOnly]
        private int _cycleSecond;

        [SerializeField, ReadOnly]
        private int _cycleMinute;

        [SerializeField, ReadOnly]
        private int _cycleHour;

        [SerializeField, ReadOnly]
        private bool _cycleSimulate;
        
        [SerializeField, ReadOnly, Space(height: 3)]
        private float _normalized;

        [Title(Constants.Settings)]
        [SerializeField, Range(0, 60), DisableInEditorMode]
        [OnValueChanged(nameof(OnSecondChanged))]
        private int _second;

        [SerializeField, Range(0, 60), DisableInEditorMode]
        [OnValueChanged(nameof(OnMinuteChanged))]
        private int _minute;

        [SerializeField, Range(0, 23), DisableInEditorMode]
        [OnValueChanged(nameof(OnHourChanged))]
        private int _hour = 12;

        [SerializeField, DisableInEditorMode]
        [OnValueChanged(nameof(OnSimulateChanged))]
        private bool _simulate;

        // FIELDS: --------------------------------------------------------------------------------

        private const string CycleInfo = "Cycle Info";
        
        private ITimeService _timeService;
        private ITimeObserver _timeObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeObserver.OnTimeUpdatedEvent += OnTimeUpdated;

        private void Start()
        {
            MyDateTime dateTime = _timeService.GetDateTime();
            _simulate = _timeService.GetSimulateState();

            UpdateFields(dateTime);
        }

        private void OnDestroy() =>
            _timeObserver.OnTimeUpdatedEvent -= OnTimeUpdated;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateFields(MyDateTime dateTime)
        {
            _cycleSecond = dateTime.Second;
            _cycleMinute = dateTime.Minute;
            _cycleHour = dateTime.Hour;
            _normalized = _timeService.GetDateTimeNormalized();
            
            UpdateSimulateField();
        }

        private void UpdateSimulateField() =>
            _cycleSimulate = _timeService.GetSimulateState();

        private void SendTimeUpdated(MyDateTime dateTime) =>
            _timeObserver.TimeUpdated(dateTime);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime) => UpdateFields(dateTime);

        private void OnSecondChanged()
        {
            MyDateTime dateTime = _timeService.GetDateTime();
            _timeService.SetDateTime(_second, dateTime.Minute, dateTime.Hour, dateTime.Day);
            UpdateFields(dateTime);
            SendTimeUpdated(dateTime);
        }

        private void OnMinuteChanged()
        {
            MyDateTime dateTime = _timeService.GetDateTime();
            _timeService.SetDateTime(dateTime.Second, _minute, dateTime.Hour, dateTime.Day);
            UpdateFields(dateTime);
            SendTimeUpdated(dateTime);
        }

        private void OnHourChanged()
        {
            MyDateTime dateTime = _timeService.GetDateTime();
            _timeService.SetDateTime(dateTime.Second, dateTime.Minute, _hour, dateTime.Day);
            UpdateFields(dateTime);
            SendTimeUpdated(dateTime);
        }

        private void OnSimulateChanged()
        {
            _timeService.ToggleSimulate(_simulate);
            UpdateSimulateField();
        }
    }
}