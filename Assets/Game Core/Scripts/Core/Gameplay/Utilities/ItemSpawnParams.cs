using System;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Utilities
{
    public class ItemSpawnParams<TItemObject> where TItemObject : ItemObjectBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        private ItemSpawnParams()
        {
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public AssetReference AssetReference { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public ulong OwnerID { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> FailCallbackEvent;
        public event Action<TItemObject> SuccessCallbackEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetAssetReference(AssetReference assetReference) =>
            AssetReference = assetReference;

        public void SendFailCallback(string reason)
        {
            Debug.LogError(reason);
            FailCallbackEvent?.Invoke(reason);
        }

        public void SendSuccessCallback(TItemObject entity) =>
            SuccessCallbackEvent?.Invoke(entity);

        // INNER CLASSES: -------------------------------------------------------------------------

        public class Builder
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public Builder()
            {
                _spawnParams = new ItemSpawnParams<TItemObject>
                {
                    Rotation = Quaternion.identity,
                    OwnerID = NetworkHorror.ServerID
                };
            }

            // FIELDS: --------------------------------------------------------------------------------

            private readonly ItemSpawnParams<TItemObject> _spawnParams;

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

            public Builder SetSuccessCallback(Action<TItemObject> successCallback)
            {
                _spawnParams.SuccessCallbackEvent += successCallback;
                return this;
            }

            public ItemSpawnParams<TItemObject> Build() => _spawnParams;
        }
    }
}