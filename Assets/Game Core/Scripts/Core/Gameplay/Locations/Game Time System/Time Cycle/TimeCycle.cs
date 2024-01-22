using System;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Time;
using GameCore.Enums;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Locations.GameTime
{
    public class TimeCycle : ITimeCycle, IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TimeCycle(ITimeCycleDecorator timeCycleDecorator, IGameplayConfigsProvider gameplayConfigsProvider,
            Sun sun)
        {
            _timeCycleDecorator = timeCycleDecorator;
            _timeConfig = gameplayConfigsProvider.GetTimeConfig();
            _sun = sun.Light;
            _sunTransform = _sun.transform;
            _simulate = _timeConfig.Simulate;
            _stopAtNight = _timeConfig.StopAtNight;
            
            SetDateTime(_timeConfig.Second, _timeConfig.Minute, _timeConfig.Hour);

            OnHourPassedEvent += _timeCycleDecorator.SendHourPassedEvent;
            
            _timeCycleDecorator.OnTickEvent += Tick;
            _timeCycleDecorator.OnSyncDateTimeEvent += SyncDateTime;
            _timeCycleDecorator.OnGetDateTimeEvent += GetDateTime;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private int Second
        {
            get => _second;
            set => SetDateTime(second: value, _minute, _hour);
        }

        private int Minute
        {
            get => _minute;
            set => SetDateTime(_second, minute: value, _hour);
        }

        private int Hour
        {
            get => _hour;
            set => SetDateTime(_second, _minute, hour: value);
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DateTime> OnTimeUpdatedEvent;
        public event Action OnHourPassedEvent;

        private readonly ITimeCycleDecorator _timeCycleDecorator;
        private readonly TimeConfigMeta _timeConfig;
        private readonly Transform _sunTransform;
        private readonly Light _sun;

        private DayTime _dayTime;
        private DateTime _date;
        private float _internalTimeOverflow;
        private int _second;
        private int _minute;
        private int _hour;
        private bool _simulate;
        private bool _stopAtNight;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            UpdateVisual();
            SendTimeUpdatedEvent();
        }
        
        public void Dispose()
        {
            OnHourPassedEvent -= _timeCycleDecorator.SendHourPassedEvent;
            
            _timeCycleDecorator.OnTickEvent -= Tick;
            _timeCycleDecorator.OnSyncDateTimeEvent -= SyncDateTime;
            _timeCycleDecorator.OnGetDateTimeEvent -= GetDateTime;
        }
        
        public void Tick()
        {
            if (!_simulate)
                return;

            if (_stopAtNight && _hour == 0)
            {
                _second = 0;
                _minute = 0;

                UpdateVisual();
                return;
            }

            UpdateModule();
            SendTimeUpdatedEvent();
        }

        public void SetDateTime(int second, int minute, int hour)
        {
            _second = second;
            _minute = minute;
            _hour = hour;

            DateTime currentTime = new();
            currentTime = currentTime.AddHours(hour);
            currentTime = currentTime.AddMinutes(minute);
            currentTime = currentTime.AddSeconds(second);

            if (_date.Hour != currentTime.Hour)
            {
                // Hour passed
            }

            if (_date.Minute != currentTime.Minute)
                SendHourPassedEvent();

            _date = currentTime;
            _second = _date.Second;
            _minute = _date.Minute;
            _hour = _date.Hour;
        }
        
        public void SyncDateTime(MyDateTime dateTime)
        {
            SetDateTime(dateTime.Second, dateTime.Minute, dateTime.Hour);
            UpdateVisual();
        }

        public void SetMidnight()
        {
            _second = 0;
            _minute = 0;
            _hour = 0;

            UpdateModule();
        }

        public void SetSunrise()
        {
            _second = 0;
            _minute = 0;
            _hour = 6;

            UpdateModule();
        }

        public void ToggleSimulate(bool simulate) =>
            _simulate = simulate;

        public DateTime GetDateTime() => _date;

        public bool GetSimulateState() => _simulate;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateModule()
        {
            float cycleDurationInMinutes = _timeConfig.CycleDurationInMinutes;
            float cycleLengthModifier = _timeConfig.CycleLengthModifier;
            float t = 24f / 60f / (cycleDurationInMinutes * cycleLengthModifier);
            t = t * 3600f * Time.deltaTime;

            if (t < 1f)
                _internalTimeOverflow += t;
            else
                _internalTimeOverflow = t;

            Second += (int)_internalTimeOverflow;

            if (_internalTimeOverflow >= 1f)
                _internalTimeOverflow = 0f;

            SetDateTime(_second, _minute, _hour);
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            float timeOfDay = GetTimeOfDay();

            UpdateDayTime();
            UpdateLighting(timeOfDay);
            UpdateSunRotation(timeOfDay);
        }

        private void UpdateDayTime()
        {
            switch (_hour)
            {
                case >= 6 and <= 8:
                    _dayTime = DayTime.Sunrise;
                    return;
                case > 8 and < 18:
                    _dayTime = DayTime.Day;
                    return;
                case >= 18 and < 22:
                    _dayTime = DayTime.Sunset;
                    return;
                case >= 22 or < 6:
                    _dayTime = DayTime.Night;
                    break;
            }
        }

        private void UpdateLighting(float timeOfDay)
        {
            RenderSettings.ambientEquatorColor = _timeConfig.EquatorColor.Evaluate(timeOfDay);
            RenderSettings.ambientSkyColor = _timeConfig.SkyColor.Evaluate(timeOfDay);

            _sun.color = _timeConfig.SunColor.Evaluate(timeOfDay);
        }

        private void UpdateSunRotation(float timeOfDay)
        {
            Quaternion sunRotation = _sunTransform.rotation;
            float x = Mathf.Lerp(-90f, 270f, timeOfDay);
            float y = sunRotation.y;
            float z = sunRotation.z;

            _sunTransform.rotation = Quaternion.Euler(x, y, z);
        }

        private float GetTimeOfDay()
        {
            const int secondsInDay = 86400;
            const int secondsInHour = 3600;
            const int secondsInMinute = 60;

            float totalSeconds = _hour * secondsInHour +
                                 _minute * secondsInMinute +
                                 _second;

            float timeOfDay = Mathf.Clamp01(totalSeconds / (float)secondsInDay);
            return timeOfDay;
        }

        private void SendTimeUpdatedEvent() =>
            OnTimeUpdatedEvent?.Invoke(_date);

        private void SendHourPassedEvent() =>
            OnHourPassedEvent?.Invoke();
    }
}