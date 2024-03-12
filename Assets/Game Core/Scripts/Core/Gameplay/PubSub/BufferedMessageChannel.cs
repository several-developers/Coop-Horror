using System;

namespace GameCore.Gameplay.PubSub
{
    public class BufferedMessageChannel<T> : MessageChannel<T>, IBufferedMessageChannel<T>
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public bool HasBufferedMessage { get; private set; }
        public T BufferedMessage { get; private set; }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override void Publish(T message)
        {
            HasBufferedMessage = true;
            BufferedMessage = message;
            base.Publish(message);
        }

        public override IDisposable Subscribe(Action<T> handler)
        {
            IDisposable subscription = base.Subscribe(handler);

            if (HasBufferedMessage)
                handler?.Invoke(BufferedMessage);

            return subscription;
        }
    }
}