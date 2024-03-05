using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace GameCore.Gameplay.PubSub
{
    public class MessageChannel<T> : IMessageChannel<T>
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsDisposed { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<Action<T>> _messageHandlers = new();

        /// This dictionary of handlers to be either added or removed is used to prevent problems from immediate
        /// modification of the list of subscribers. It could happen if one decides to unsubscribe in a message handler
        /// etc.A true value means this handler should be added, and a false one means it should be removed
        private readonly Dictionary<Action<T>, bool> _pendingHandlers = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            
            IsDisposed = true;
            _messageHandlers.Clear();
            _pendingHandlers.Clear();
        }

        public virtual void Publish(T message)
        {
            foreach (Action<T> handler in _pendingHandlers.Keys)
            {
                if (_pendingHandlers[handler])
                    _messageHandlers.Add(handler);
                else
                    _messageHandlers.Remove(handler);
            }

            _pendingHandlers.Clear();

            foreach (Action<T> messageHandler in _messageHandlers)
                messageHandler?.Invoke(message);
        }

        public virtual IDisposable Subscribe(Action<T> handler)
        {
            Assert.IsTrue(condition: !IsSubscribed(handler),
                message: "Attempting to subscribe with the same handler more than once");

            if (_pendingHandlers.ContainsKey(handler))
            {
                if (!_pendingHandlers[handler])
                    _pendingHandlers.Remove(handler);
            }
            else
            {
                _pendingHandlers[handler] = true;
            }

            var subscription = new DisposableSubscription<T>(this, handler);
            return subscription;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (!IsSubscribed(handler))
                return;
            
            if (_pendingHandlers.ContainsKey(handler))
            {
                if (_pendingHandlers[handler])
                    _pendingHandlers.Remove(handler);
            }
            else
            {
                _pendingHandlers[handler] = false;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsSubscribed(Action<T> handler)
        {
            var isPendingRemoval = _pendingHandlers.ContainsKey(handler) && !_pendingHandlers[handler];
            var isPendingAdding = _pendingHandlers.ContainsKey(handler) && _pendingHandlers[handler];
            return _messageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
        }
    }
}