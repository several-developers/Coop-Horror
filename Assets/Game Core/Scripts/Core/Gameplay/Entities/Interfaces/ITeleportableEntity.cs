﻿using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    public interface ITeleportableEntity : IEntity
    {
        void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false);
        void SetEntityLocation(EntityLocation entityLocation);
        void SetFloor(Floor floor);
    }
}