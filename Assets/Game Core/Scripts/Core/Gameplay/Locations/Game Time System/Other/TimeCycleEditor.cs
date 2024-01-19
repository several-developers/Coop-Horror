using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Locations.GameTime
{
    public class TimeCycleEditor : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle) =>
            _timeCycle = timeCycle;

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
        
        private ITimeCycle _timeCycle;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeCycle.OnTimeUpdatedEvent += OnTimeUpdated;

        private void Start()
        {
            DateTime dateTime = _timeCycle.GetDateTime();
            _simulate = _timeCycle.GetSimulateState();

            UpdateFields(dateTime);
        }

        private void OnDestroy() =>
            _timeCycle.OnTimeUpdatedEvent -= OnTimeUpdated;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateFields(DateTime dateTime)
        {
            _cycleSecond = dateTime.Second;
            _cycleMinute = dateTime.Minute;
            _cycleHour = dateTime.Hour;
            
            UpdateSimulateField();
        }

        private void UpdateSimulateField() =>
            _cycleSimulate = _timeCycle.GetSimulateState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(DateTime dateTime) => UpdateFields(dateTime);

        private void OnSecondChanged()
        {
            DateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(_second, dateTime.Minute, dateTime.Hour);
            UpdateFields(dateTime);
        }

        private void OnMinuteChanged()
        {
            DateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(dateTime.Second, _minute, dateTime.Hour);
            UpdateFields(dateTime);
        }

        private void OnHourChanged()
        {
            DateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(dateTime.Second, dateTime.Minute, _hour);
            UpdateFields(dateTime);
        }

        private void OnSimulateChanged()
        {
            _timeCycle.ToggleSimulate(_simulate);
            UpdateSimulateField();
        }
    }
}