using System;

namespace GameCore.Gameplay.PubSub
{
    /// <summary>
    /// This class is a handle to an active Message Channel subscription and
    /// when disposed it unsubscribes from said channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DisposableSubscription<T> : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public DisposableSubscription(IMessageChannel<T> messageChannel, Action<T> handler)
        {
            _messageChannel = messageChannel;
            _handler = handler;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IMessageChannel<T> _messageChannel;
        private Action<T> _handler;
        private bool _isDisposed;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _isDisposed = true;

            if (!_messageChannel.IsDisposed)
                _messageChannel.Unsubscribe(_handler);

            _handler = null;
            _messageChannel = null;
        }
    }
}