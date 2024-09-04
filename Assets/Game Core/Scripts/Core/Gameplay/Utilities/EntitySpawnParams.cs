using System;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Monsters.GoodClown;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.Utilities
{
    public class Test
    {
        public void Test2()
        {
            var entitySpawnParams = new EntitySpawnParams<GoodClownEntity>.Builder()
                .SetWorldPosition(Vector3.zero)
                .SetRotation(Quaternion.identity)
                .SetOwnerID(123)
                .SetFailCallback(reason => { Debug.LogError(reason); })
                .SetSuccessCallback(entity => { Type type = entity.GetType(); })
                .Build();
            
        }
    }

    public class EntitySpawnParams
    {
        
    }
    public class EntitySpawnParams<TEntity> : EntitySpawnParams where TEntity : Entity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        private EntitySpawnParams(Vector3 worldPosition, Quaternion rotation, ulong ownerID,
            Action<string> failCallback, Action<TEntity> successCallback)
        {
            WorldPosition = worldPosition;
            Rotation = rotation;
            OwnerID = ownerID;
            FailCallback = failCallback;
            SuccessCallback = successCallback;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector3 WorldPosition { get; }
        public Quaternion Rotation { get; }
        public ulong OwnerID { get; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> FailCallback;
        public event Action<TEntity> SuccessCallback;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SendFailCallback(string reason) =>
            FailCallback?.Invoke(reason);

        public void SendSuccessCallback(TEntity entity) =>
            SuccessCallback?.Invoke(entity);

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

            private Vector3 _worldPosition;
            private Quaternion _rotation;
            private ulong _ownerID;
            private Action<string> _failCallback;
            private Action<TEntity> _successCallback;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public Builder SetWorldPosition(Vector3 worldPosition)
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

            public EntitySpawnParams Build()
            {
                var spawnParams = new EntitySpawnParams<TEntity>(
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