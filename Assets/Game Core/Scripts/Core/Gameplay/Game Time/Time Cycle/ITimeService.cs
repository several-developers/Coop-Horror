namespace GameCore.Gameplay.GameTimeManagement
{
    public interface ITimeService
    {
        void Tick();
        void SetDateTime(int second, int dateTimeMinute, int dateTimeHour, int day);
        void SyncDateTime(MyDateTime dateTime);
        void SetMidnight();
        void SetSunrise();
        void ToggleSimulate(bool simulate);
        void IncreaseDay();
        MyDateTime GetDateTime();
        float GetDateTimeNormalized();
        float GetHourDurationInSeconds();
        float GetMinuteDurationInSeconds();
        int GetCurrentTimeInMinutes();
        bool GetSimulateState();
    }
}