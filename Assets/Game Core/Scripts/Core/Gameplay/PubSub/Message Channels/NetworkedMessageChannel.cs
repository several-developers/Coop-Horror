using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.PubSub
{
    /// <summary>
    /// This type of message channel allows the server to publish a message that will be sent to clients as well as
    /// being published locally. Clients and the server both can subscribe to it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkedMessageChannel<T> : MessageChannel<T> where T : unmanaged, INetworkSerializeByMemcpy
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public NetworkedMessageChannel()
        {
            _name = $"{typeof(T).FullName}NetworkMessageChannel";
            
            _networkManager = NetworkManager.Singleton;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            
            if (_networkManager.IsListening)
                RegisterHandler();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly NetworkManager _networkManager;
        private readonly string _name;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Publish(T message)
        {
            if (_networkManager.IsServer)
            {
                // send message to clients, then publish locally
                SendMessageThroughNetwork(message);
                base.Publish(message);
            }
            else
            {
                Debug.LogError("Only a server can publish in a NetworkedMessageChannel");
            }
        }
        
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                bool isValid = _networkManager != null && _networkManager.CustomMessagingManager != null;
                
                if (isValid)
                    _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(_name);
            }

            base.Dispose();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnClientConnected(ulong clientId) => RegisterHandler();

        private void RegisterHandler()
        {
            if (_networkManager.IsServer)
                return;
            
            // Only register message handler on clients
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(_name, ReceiveMessageThroughNetwork);
        }

        private void SendMessageThroughNetwork(T message)
        {
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<T>(), Allocator.Temp);
            writer.WriteValueSafe(message);
            _networkManager.CustomMessagingManager.SendNamedMessageToAll(_name, writer);
        }

        private void ReceiveMessageThroughNetwork(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out T message);
            base.Publish(message);
        }
    }
}