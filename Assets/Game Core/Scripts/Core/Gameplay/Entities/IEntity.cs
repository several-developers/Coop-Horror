using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    public interface IEntity
    {
        void Teleport(Vector3 position, Quaternion rotation);
        Transform GetTransform();
    }
}