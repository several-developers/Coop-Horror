using System;

namespace GameCore.Gameplay.PubSub
{
    public interface ISubscriber<T>
    {
        IDisposable Subscribe(Action<T> handler);
        void Unsubscribe(Action<T> handler);
    }
}