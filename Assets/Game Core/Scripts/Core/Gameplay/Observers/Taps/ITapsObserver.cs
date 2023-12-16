using System;

namespace GameCore.Gameplay.Observers.Taps
{
    public interface ITapsObserver
    {
        event Action OnTapDownEvent;
        event Action OnTapUpEvent;
        void SendTapDownEvent();
        void SendTapUpEvent();
    }
}