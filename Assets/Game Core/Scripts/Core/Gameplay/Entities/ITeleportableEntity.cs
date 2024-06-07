using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    public interface ITeleportableEntity : IEntity
    {
        void Teleport(Vector3 position, Quaternion rotation);
    }
}