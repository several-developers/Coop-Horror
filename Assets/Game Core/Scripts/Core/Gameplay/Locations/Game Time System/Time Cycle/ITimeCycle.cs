using System;

namespace GameCore.Gameplay.Locations.GameTime
{
    public interface ITimeCycle
    {
        event Action<DateTime> OnTimeUpdatedEvent;
        event Action OnHourPassedEvent;
        void Tick();
        void SetDateTime(int second, int dateTimeMinute, int dateTimeHour);
        void SyncDateTime(MyDateTime dateTime);
        void SetMidnight();
        void SetSunrise();
        void ToggleSimulate(bool simulate);
        DateTime GetDateTime();
        bool GetSimulateState();
    }
}