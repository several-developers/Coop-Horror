using System;

namespace GameCore.Gameplay.GameTimeManagement
{
    public interface IGameTimeManagerDecorator
    {
        event Action OnIncreaseDayEvent;
        void IncreaseDay();
    }
}