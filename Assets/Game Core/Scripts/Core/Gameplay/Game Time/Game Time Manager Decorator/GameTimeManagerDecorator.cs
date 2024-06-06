using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManagerDecorator : IGameTimeManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public event Action OnSetMidnightInnerEvent = delegate { };
        public event Action OnResetDayInnerEvent = delegate { };

        public void SetMidnight() =>
            OnSetMidnightInnerEvent.Invoke();

        public void ResetDay() =>
            OnResetDayInnerEvent.Invoke();
    }
}