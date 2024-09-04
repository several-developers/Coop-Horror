using System;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Utilities
{
    public class EntitySpawnParams<TEntity> where TEntity : Entity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        private EntitySpawnParams(AssetReference assetReference, Vector3 worldPosition, Quaternion rotation,
            ulong ownerID, Action<string> failCallback, Action<TEntity> successCallback)
        {
            AssetReference = assetReference;
            WorldPosition = worldPosition;
            Rotation = rotation;
            OwnerID = ownerID;
            FailCallbackEvent += failCallback;
            SuccessCallbackEvent += successCallback;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public AssetReference AssetReference { get; private set; }
        public Vector3 WorldPosition { get; }
        public Quaternion Rotation { get; }
        public ulong OwnerID { get; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> FailCallbackEvent;
        public event Action<TEntity> SuccessCallbackEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetAssetReference(AssetReference assetReference) =>
            AssetReference = assetReference;

        public void SendFailCallback(string reason)
        {
            Debug.LogError(reason);
            FailCallbackEvent?.Invoke(reason);
        }

        public void SendSuccessCallback(TEntity entity) =>
            SuccessCallbackEvent?.Invoke(entity);

        // INNER CLASSES: -------------------------------------------------------------------------

        public class Builder
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public Builder()
            {
                _rotation = Quaternion.identity;
                _ownerID = NetworkHorror.ServerID;
            }

            // FIELDS: --------------------------------------------------------------------------------

            private AssetReference _assetReference;
            private Vector3 _worldPosition;
            private Quaternion _rotation;
            private ulong _ownerID;
            private Action<string> _failCallback;
            private Action<TEntity> _successCallback;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public Builder SetAssetReference(AssetReference assetReference)
            {
                _assetReference = assetReference;
                return this;
            }

            public Builder SetSpawnPosition(Vector3 worldPosition)
            {
                _worldPosition = worldPosition;
                return this;
            }

            public Builder SetRotation(Quaternion rotation)
            {
                _rotation = rotation;
                return this;
            }

            public Builder SetOwnerID(ulong ownerID)
            {
                _ownerID = ownerID;
                return this;
            }

            public Builder SetFailCallback(Action<string> failCallback)
            {
                _failCallback = failCallback;
                return this;
            }

            public Builder SetSuccessCallback(Action<TEntity> successCallback)
            {
                _successCallback = successCallback;
                return this;
            }

            public EntitySpawnParams<TEntity> Build()
            {
                var spawnParams = new EntitySpawnParams<TEntity>(
                    _assetReference,
                    _worldPosition,
                    _rotation,
                    _ownerID,
                    _failCallback,
                    _successCallback
                );

                return spawnParams;
            }
        }
    }
}