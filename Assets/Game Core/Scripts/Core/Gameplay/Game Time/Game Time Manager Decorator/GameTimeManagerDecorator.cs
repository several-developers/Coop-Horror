using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManagerDecorator : IGameTimeManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnSetSunriseInnerEvent = delegate { };
        public event Action OnSetMidnightInnerEvent = delegate { };
        public event Action OnIncreaseDayInnerEvent = delegate { };
        public event Action OnResetDayInnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetSunrise() =>
            OnSetSunriseInnerEvent.Invoke();

        public void SetMidnight() =>
            OnSetMidnightInnerEvent.Invoke();

        public void IncreaseDay() =>
            OnIncreaseDayInnerEvent.Invoke();

        public void ResetDay() =>
            OnResetDayInnerEvent.Invoke();
    }
}