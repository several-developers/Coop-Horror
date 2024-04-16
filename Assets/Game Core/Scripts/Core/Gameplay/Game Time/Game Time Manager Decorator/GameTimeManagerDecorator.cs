using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class GameTimeManagerDecorator : IGameTimeManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnIncreaseDayEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void IncreaseDay() =>
            OnIncreaseDayEvent.Invoke();
    }
}