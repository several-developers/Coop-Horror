using System;
using GameCore.Gameplay.GameTimeManagement;

namespace GameCore.Observers.Gameplay.Time
{
    public class TimeObserver : ITimeObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<MyDateTime> OnTimeUpdatedEvent = delegate { };
        public event Action OnHourPassedEvent = delegate { };
        public event Action OnMinutePassedEvent = delegate { };
        public event Func<MyDateTime> GetDateTimeInnerEvent = () => new MyDateTime();
        public event Func<float> GetDateTimeNormalizedInnerEvent = () => 0f;
        public event Func<float> GetHourDurationInSecondsInnerEvent = () => 1f;
        public event Func<float> GetMinuteDurationInSecondsInnerEvent = () => 1f;
        public event Func<int> GetCurrentTimeInMinutesInnerEvent = () => 0;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TimeUpdated(MyDateTime dateTime) =>
            OnTimeUpdatedEvent.Invoke(dateTime);

        public void HourPassed() =>
            OnHourPassedEvent.Invoke();

        public void MinutePassed() =>
            OnMinutePassedEvent.Invoke();

        public MyDateTime GetDateTime() =>
            GetDateTimeInnerEvent.Invoke();

        public float GetDateTimeNormalized() =>
            GetDateTimeNormalizedInnerEvent.Invoke();

        public float GetHourDurationInSeconds() =>
            GetHourDurationInSecondsInnerEvent.Invoke();

        public float GetMinuteDurationInSeconds() =>
            GetMinuteDurationInSecondsInnerEvent.Invoke();

        public int GetCurrentTimeInMinutes() =>
            GetCurrentTimeInMinutesInnerEvent.Invoke();
    }
}