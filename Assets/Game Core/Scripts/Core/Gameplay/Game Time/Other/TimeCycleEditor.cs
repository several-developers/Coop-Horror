﻿using GameCore.Observers.Gameplay.Time;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class TimeCycleEditor : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle, ITimeObserver timeObserver)
        {
            _timeCycle = timeCycle;
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
        
        private ITimeCycle _timeCycle;
        private ITimeObserver _timeObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeObserver.OnTimeUpdatedEvent += OnTimeUpdated;

        private void Start()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            _simulate = _timeCycle.GetSimulateState();

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
            _normalized = _timeCycle.GetDateTimeNormalized();
            
            UpdateSimulateField();
        }

        private void UpdateSimulateField() =>
            _cycleSimulate = _timeCycle.GetSimulateState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime) => UpdateFields(dateTime);

        private void OnSecondChanged()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(_second, dateTime.Minute, dateTime.Hour, dateTime.Day);
            UpdateFields(dateTime);
        }

        private void OnMinuteChanged()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(dateTime.Second, _minute, dateTime.Hour, dateTime.Day);
            UpdateFields(dateTime);
        }

        private void OnHourChanged()
        {
            MyDateTime dateTime = _timeCycle.GetDateTime();
            _timeCycle.SetDateTime(dateTime.Second, dateTime.Minute, _hour, dateTime.Day);
            UpdateFields(dateTime);
        }

        private void OnSimulateChanged()
        {
            _timeCycle.ToggleSimulate(_simulate);
            UpdateSimulateField();
        }
    }
}