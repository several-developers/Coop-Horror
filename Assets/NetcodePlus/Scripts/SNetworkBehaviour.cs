using System;
using UnityEngine;
using Unity.Netcode;

namespace NetcodePlus
{
    /// <summary>
    /// Improved version of NetworkBehaviour
    /// </summary>

    public abstract class SNetworkBehaviour : MonoBehaviour
    {
        private SNetworkObject _sNetworkObject;        //If your script inherits from SNetworkBehaviour, it should have a SNetworkObject component on it
        private ushort _behaviourID;        //ID of the behaviour
        private byte[] _extra = Array.Empty<byte>();     //Extra data transfered from server to client when spawned

        protected virtual void Awake()
        {
            _sNetworkObject = GetComponentInParent<SNetworkObject>();
            
            if (_sNetworkObject != null)
            {
                _sNetworkObject.onReady += OnReady;
                _sNetworkObject.onBeforeSpawn += OnBeforeSpawn;
                _sNetworkObject.onSpawn += OnSpawn;
                _sNetworkObject.onDespawn += OnDespawn;
            }
            else
            {
                Debug.LogError(gameObject.name + " should have a SNetworkObject component if a script inherits from SNetworkBehaviour");
            }
        }

        protected virtual void OnReady()
        {
            //Function will run after connection was fully established (and all data loaded), unlike Spawn() this function will run only once
        }

        protected virtual void OnBeforeSpawn()
        {
            //Function will run before spawning (server only)
        }

        protected virtual void OnSpawn()
        {
            //Function will run after spawned
        }

        protected virtual void OnDespawn()
        {
            //Function will run before despawned
        }

        public void SetBehaviourId(ushort id) =>
            _behaviourID = id;

        public void SetSpawnData(byte[] data) =>
            _extra = data;

        public byte[] GetSpawnData() => _extra;

        public void SetSpawnData(int data) =>
            _extra = NetworkTool.SerializeInt32(data);

        public int GetSpawnDataInt32() =>
            NetworkTool.DeserializeInt32(_extra);

        public void SetSpawnData(ulong data) =>
            _extra = NetworkTool.SerializeUInt64(data);

        public ulong GetSpawnDataUInt64() =>
            NetworkTool.DeserializeUInt64(_extra);

        public void SetSpawnData(string data) =>
            _extra = NetworkTool.SerializeString(data);

        public string GetSpawnDataString() =>
            NetworkTool.DeserializeString(_extra);

        //Extra data transfered from server to client when spawned
        public void SetSpawnData<T>(T data) where T : INetworkSerializable, new() =>
            _extra = NetworkTool.NetSerialize(data);

        public T GetSpawnData<T>() where T : INetworkSerializable, new() =>
            NetworkTool.NetDeserialize<T>(_extra);

        public T Get<T>() where T : SNetworkBehaviour =>
            this as T;

        public SNetworkObject NetObject => _sNetworkObject;

        public ulong NetworkId => _sNetworkObject != null ? _sNetworkObject.NetworkId : 0;
        public ulong OwnerId => _sNetworkObject != null ? _sNetworkObject.OwnerId : 0;
        public ushort BehaviourId => _behaviourID;

        public bool IsServer => _sNetworkObject != null && _sNetworkObject.IsServer;
        public bool IsClient => _sNetworkObject != null && _sNetworkObject.IsClient;
        public bool IsOwner => _sNetworkObject != null && _sNetworkObject.IsOwner;

        public bool IsSpawned => _sNetworkObject != null && _sNetworkObject.IsSpawned;
        public bool IsReady => _sNetworkObject != null && _sNetworkObject.IsReady;
    }

    [Serializable]
    public struct SNetworkBehaviourRef : INetworkSerializable
    {
        public ulong _netID;
        public ushort _behaviourID;

        public SNetworkBehaviourRef(SNetworkBehaviour behaviour)
        {
            if (behaviour != null)
            {
                _netID = behaviour.NetworkId;
                _behaviourID = behaviour.BehaviourId;
            }
            else
            {
                _netID = 0;
                _behaviourID = 0;
            }
        }

        public SNetworkBehaviour Get()
        {
            if (_netID == 0)
                return null;
            
            return NetworkSpawner.Get().GetSpawnedBehaviour(_netID, _behaviourID);
        }

        public T Get<T>() where T : SNetworkBehaviour
        {
            if (_netID == 0)
                return null;
            
            SNetworkBehaviour sb = NetworkSpawner.Get().GetSpawnedBehaviour(_netID, _behaviourID);
            
            if (sb is T sNetworkBehaviour)
                return sNetworkBehaviour;
            
            return null;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _netID);
            serializer.SerializeValue(ref _behaviourID);
        }
    }
}
