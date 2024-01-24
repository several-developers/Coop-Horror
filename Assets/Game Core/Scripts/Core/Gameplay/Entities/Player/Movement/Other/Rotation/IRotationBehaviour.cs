using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Rotation
{
    public interface IRotationBehaviour
    {
        void Rotate();
        void SetLookVector(Vector2 lookVector);
        void ChangeLockCameraPosition();
    }
}