using System;
using GameCore.Gameplay.GameTimeManagement;

namespace GameCore.Observers.Gameplay.Time
{
    public interface ITimeObserver
    {
        event Action<MyDateTime> OnTimeUpdatedEvent;
        event Action OnHourPassedEvent;
        event Action OnMinutePassedEvent;
        
        event Func<MyDateTime> GetDateTimeInnerEvent;
        event Func<float> GetDateTimeNormalizedInnerEvent;
        event Func<float> GetHourDurationInSecondsInnerEvent;
        event Func<float> GetMinuteDurationInSecondsInnerEvent;
        event Func<int> GetCurrentTimeInMinutesInnerEvent;

        void TimeUpdated(MyDateTime dateTime);
        void HourPassed();
        void MinutePassed();
        
        MyDateTime GetDateTime();
        float GetDateTimeNormalized();
        float GetHourDurationInSeconds();
        float GetMinuteDurationInSeconds();
        int GetCurrentTimeInMinutes();
    }
}