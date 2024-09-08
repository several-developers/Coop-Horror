using System;
using GameCore.Gameplay.Network;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Utilities
{
    public class SpawnParams<T> where T : class
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        private SpawnParams()
        {
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public AssetReference AssetReference { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public ulong OwnerID { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> FailCallbackEvent;
        public event Action<T> SetupInstanceEvent;
        public event Action<T> SuccessCallbackEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetAssetReference(AssetReference assetReference) =>
            AssetReference = assetReference;

        public void SendFailCallback(string reason)
        {
            Debug.LogError(reason);
            FailCallbackEvent?.Invoke(reason);
        }

        public void SendSuccessCallback(T entity) =>
            SuccessCallbackEvent?.Invoke(entity);
        
        public void SendSetupInstance(T entity) =>
            SetupInstanceEvent?.Invoke(entity);

        // INNER CLASSES: -------------------------------------------------------------------------

        public class Builder
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public Builder()
            {
                _spawnParams = new SpawnParams<T>
                {
                    Rotation = Quaternion.identity,
                    OwnerID = NetworkHorror.ServerID
                };
            }

            // FIELDS: --------------------------------------------------------------------------------

            private readonly SpawnParams<T> _spawnParams;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public Builder SetAssetReference(AssetReference assetReference)
            {
                _spawnParams.AssetReference = assetReference;
                return this;
            }

            public Builder SetSpawnPosition(Vector3 worldPosition)
            {
                _spawnParams.WorldPosition = worldPosition;
                return this;
            }

            public Builder SetRotation(Quaternion rotation)
            {
                _spawnParams.Rotation = rotation;
                return this;
            }

            public Builder SetOwnerID(ulong ownerID)
            {
                _spawnParams.OwnerID = ownerID;
                return this;
            }

            public Builder SetFailCallback(Action<string> failCallback)
            {
                _spawnParams.FailCallbackEvent += failCallback;
                return this;
            }

            public Builder SetSuccessCallback(Action<T> successCallback)
            {
                _spawnParams.SuccessCallbackEvent += successCallback;
                return this;
            }

            public SpawnParams<T> Build() => _spawnParams;
        }
    }
}