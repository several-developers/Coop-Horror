using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public interface ITimeCycle
    {
        event Action<MyDateTime> OnTimeUpdatedEvent;
        event Action OnHourPassedEvent;
        void Tick();
        void SetDateTime(int second, int dateTimeMinute, int dateTimeHour, int day);
        void SyncDateTime(MyDateTime dateTime);
        void SetMidnight();
        void SetSunrise();
        void ToggleSimulate(bool simulate);
        void IncreaseDay();
        MyDateTime GetDateTime();
        bool GetSimulateState();
    }
}