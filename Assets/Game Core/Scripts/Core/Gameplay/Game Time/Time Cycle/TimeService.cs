using System;
using GameCore.Configs.Gameplay.Time;
using GameCore.Enums.Gameplay;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Time;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class TimeService : ITimeService, IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TimeService(ITimeObserver timeObserver, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _timeObserver = timeObserver;
            _timeConfig = gameplayConfigsProvider.GetConfig<TimeConfigMeta>();
            _simulate = _timeConfig.Simulate;
            _stopAtNight = _timeConfig.StopAtNight;

            SetDateTime(_timeConfig.StartAtSecond, _timeConfig.StartAtMinute, _timeConfig.StartAtHour, day: 0);
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private int Second
        {
            get => _second;
            set => SetDateTime(second: value, _minute, _hour, _day);
        }

        private int Minute
        {
            get => _minute;
            set => SetDateTime(_second, minute: value, _hour, _day);
        }

        private int Hour
        {
            get => _hour;
            set => SetDateTime(_second, _minute, hour: value, _day);
        }

        private int Day
        {
            get => _day;
            set => SetDateTime(_second, _minute, _hour, day: value);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ITimeObserver _timeObserver;
        private readonly TimeConfigMeta _timeConfig;

        private DayTime _dayTime;
        private DateTime _date;
        private float _internalTimeOverflow;
        private int _second;
        private int _minute;
        private int _hour;
        private int _day;
        private bool _simulate;
        private bool _stopAtNight;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            _timeObserver.GetDateTimeInnerEvent += GetDateTime;
            _timeObserver.GetDateTimeNormalizedInnerEvent += GetDateTimeNormalized;
            _timeObserver.GetHourDurationInSecondsInnerEvent += GetHourDurationInSeconds;
            _timeObserver.GetMinuteDurationInSecondsInnerEvent += GetMinuteDurationInSeconds;
            _timeObserver.GetCurrentTimeInMinutesInnerEvent += GetCurrentTimeInMinutes;
            
            UpdateDayTime();
            SendTimeUpdatedEvent();
        }

        public void Dispose()
        {
            _timeObserver.GetDateTimeInnerEvent -= GetDateTime;
            _timeObserver.GetDateTimeNormalizedInnerEvent -= GetDateTimeNormalized;
            _timeObserver.GetHourDurationInSecondsInnerEvent -= GetHourDurationInSeconds;
            _timeObserver.GetMinuteDurationInSecondsInnerEvent -= GetMinuteDurationInSeconds;
            _timeObserver.GetCurrentTimeInMinutesInnerEvent -= GetCurrentTimeInMinutes;
        }

        public void Tick()
        {
            if (!_simulate)
                return;

            if (_stopAtNight && _hour == 0)
            {
                _second = 0;
                _minute = 0;
                _hour = 0;

                UpdateDayTime();
                SendHourPassedEvent();
                SendMinutePassedEvent();
                return;
            }

            UpdateModule();
            SendTimeUpdatedEvent();
        }

        public void SetDateTime(int second, int minute, int hour, int day)
        {
            _second = second;
            _minute = minute;
            _hour = hour;
            _day = day;

            DateTime currentTime = new();
            currentTime = currentTime.AddHours(hour);
            currentTime = currentTime.AddMinutes(minute);
            currentTime = currentTime.AddSeconds(second);

            bool soundHourPassedEvent = false;
            bool soundMinutePassedEvent = false;

            if (_date.Hour != currentTime.Hour)
                soundHourPassedEvent = true;

            if (_date.Minute != currentTime.Minute)
                soundMinutePassedEvent = true;

            _date = currentTime;
            _second = _date.Second;
            _minute = _date.Minute;
            _hour = _date.Hour;
            
            if (soundHourPassedEvent)
                SendHourPassedEvent();

            if (soundMinutePassedEvent)
                SendMinutePassedEvent();
        }

        public void SyncDateTime(MyDateTime dateTime)
        {
            SetDateTime(dateTime.Second, dateTime.Minute, dateTime.Hour, dateTime.Day);
            UpdateDayTime();
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

        public void IncreaseDay()
        {
            _day += 1;
            UpdateModule();
        }

        public MyDateTime GetDateTime()
        {
            MyDateTime dateTime = new(_date.Second, _date.Minute, _date.Hour, _day);
            return dateTime;
        }

        public float GetDateTimeNormalized()
        {
            const int secondsInDay = Constants.SecondsInDay;
            const int secondsInHour = Constants.SecondsInHour;
            const int secondsInMinute = Constants.SecondsInMinute;

            float totalSeconds = _hour * secondsInHour +
                                 _minute * secondsInMinute +
                                 _second;

            float timeOfDay = Mathf.Clamp01(totalSeconds / secondsInDay);
            return timeOfDay;
        }

        public float GetHourDurationInSeconds()
        {
            const float hoursInDay = Constants.HoursInDay;
            const float secondsInMinute = Constants.SecondsInMinute;
            
            float cycleDurationInMinutes = _timeConfig.CycleDurationInMinutes;
            float cycleLengthModifier = _timeConfig.CycleLengthModifier;
            float totalCycleDurationInMinutes = cycleDurationInMinutes * cycleLengthModifier;
            float totalCycleDurationInSeconds = totalCycleDurationInMinutes * secondsInMinute;
            float hourDurationInSeconds = totalCycleDurationInSeconds / hoursInDay;
            return hourDurationInSeconds;
        }
        
        public float GetMinuteDurationInSeconds()
        {
            const float hoursInDay = Constants.HoursInDay;
            const float secondsInMinute = Constants.SecondsInMinute;
            
            float cycleDurationInMinutes = _timeConfig.CycleDurationInMinutes;
            float cycleLengthModifier = _timeConfig.CycleLengthModifier;
            float totalCycleDurationInMinutes = cycleDurationInMinutes * cycleLengthModifier;
            float totalCycleDurationInSeconds = totalCycleDurationInMinutes * secondsInMinute;
            float minuteDurationInSeconds = totalCycleDurationInSeconds / hoursInDay / secondsInMinute;
            return minuteDurationInSeconds;
        }
        
        public int GetCurrentTimeInMinutes()
        {
            const int secondsInMinute = Constants.SecondsInMinute;
            
            int hour = _date.Hour;
            int minute = _date.Minute;
            int totalMinutes = hour * secondsInMinute + minute;
            return totalMinutes;
        }

        public bool GetSimulateState() => _simulate;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateModule()
        {
            const float hoursInDay = Constants.HoursInDay;
            const float secondsInMinute = Constants.SecondsInMinute;
            const float secondsInHour = Constants.SecondsInHour;
            
            float cycleDurationInMinutes = _timeConfig.CycleDurationInMinutes;
            float cycleLengthModifier = _timeConfig.CycleLengthModifier;
            float t = hoursInDay / secondsInMinute / (cycleDurationInMinutes * cycleLengthModifier);
            t = t * secondsInHour * Time.deltaTime;

            if (t < 1f)
                _internalTimeOverflow += t;
            else
                _internalTimeOverflow = t;

            Second += (int)_internalTimeOverflow;

            if (_internalTimeOverflow >= 1f)
                _internalTimeOverflow = 0f;

            SetDateTime(_second, _minute, _hour, _day);
            UpdateDayTime();
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

        private void SendTimeUpdatedEvent()
        {
            MyDateTime dateTime = GetDateTime();
            _timeObserver.TimeUpdated(dateTime);
        }

        private void SendHourPassedEvent() =>
            _timeObserver.HourPassed();

        private void SendMinutePassedEvent() =>
            _timeObserver.MinutePassed();
    }
}