using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public interface IGameTimeManagerDecorator
    {
        event Action OnSetSunriseInnerEvent;
        event Action OnSetMidnightInnerEvent;
        event Action OnIncreaseDayInnerEvent;
        event Action OnResetDayInnerEvent;
        void SetSunrise();
        void SetMidnight();
        void IncreaseDay();
        void ResetDay();
    }
}