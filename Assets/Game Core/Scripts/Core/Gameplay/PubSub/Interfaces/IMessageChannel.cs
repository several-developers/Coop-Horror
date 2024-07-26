using System;

namespace GameCore.Gameplay.PubSub
{
    public interface IMessageChannel<T> : IPublisher<T>, ISubscriber<T>, IDisposable
    {
        bool IsDisposed { get; }
    }
}