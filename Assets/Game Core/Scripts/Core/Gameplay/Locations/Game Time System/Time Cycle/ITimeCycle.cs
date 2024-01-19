using System;

namespace GameCore.Gameplay.Locations.GameTime
{
    public interface ITimeCycle
    {
        event Action<DateTime> OnTimeUpdatedEvent;
        event Action OnHourPassedEvent;
        void UpdateLogic();
        void SyncDateTime(MyDateTime dateTime);
        void SetMidnight();
        void SetSunrise();
        void ToggleSimulate(bool simulate);
        DateTime GetDateTime();
    }
}