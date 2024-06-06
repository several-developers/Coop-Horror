using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public interface IGameTimeManagerDecorator
    {
        event Action OnSetMidnightInnerEvent;
        event Action OnResetDayInnerEvent;
        void SetMidnight();
        void ResetDay();
    }
}