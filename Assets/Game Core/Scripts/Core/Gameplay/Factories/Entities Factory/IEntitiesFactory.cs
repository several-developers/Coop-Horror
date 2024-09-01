﻿using GameCore.Gameplay.Entities;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Entities
{
    public interface IEntitiesFactory
    {
        bool TryCreateEntity<TEntityType>(Vector3 worldPosition, out Entity entity)
            where TEntityType : IEntity;

        bool TryCreateEntity<TEntityType>(Vector3 worldPosition, ulong ownerID, out Entity entity)
            where TEntityType : IEntity;

        bool TryCreateEntity<TEntityType>(Vector3 worldPosition, Quaternion rotation, out Entity entity)
            where TEntityType : IEntity;

        bool TryCreateEntity<TEntityType>(Vector3 worldPosition, Quaternion rotation, ulong ownerID, out Entity entity)
            where TEntityType : IEntity;

        bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab)
            where TEntityType : IEntity;
    }
}