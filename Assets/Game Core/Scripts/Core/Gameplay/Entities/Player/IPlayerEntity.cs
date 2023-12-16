using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Observers.Taps;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public interface IPlayerEntity : IEntity, IDamageable
    {
        void Setup(LevelManager levelManager, Camera mainCamera, ITapsObserver tapsObserver);
        void ReloadWeapon();
        Transform GetLookAtTransform();
        bool IsDead();
    }
}