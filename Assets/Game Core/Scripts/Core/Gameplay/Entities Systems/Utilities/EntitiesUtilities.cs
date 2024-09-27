using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Utilities
{
    public static class EntitiesUtilities
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static LookDirection GetLookDirection(Transform owner, Transform target)
        {
            float dot = GetLookDirectionDot(owner, target);

            LookDirection lookDirection = dot switch
            {
                < -0.707f => LookDirection.Behind,
                > 0.707f => LookDirection.InFront,
                _ => LookDirection.Side
            };

            return lookDirection;
        }

        public static float GetLookDirectionDot(Transform owner, Transform target)
        {
            Vector3 directionToTarget = Vector3.Normalize(owner.position - target.position);
            
            // -1 = behind
            // 0 = left/right
            // 0.707 = 45 degrees
            // 1 = in front
            float dot = Vector3.Dot(lhs: target.forward, rhs: directionToTarget);

            return dot;
        }

        public static void RotateToTarget(Transform owner, Transform target, float rotationSpeed)
        {
            // Вычисляем угол между текущим направлением и направлением на цель
            Vector3 direction = target.position - owner.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Интерполируем между текущим вращением и целевым вращением
            owner.rotation = Quaternion.Slerp(
                a: owner.rotation,
                b: targetRotation,
                t: rotationSpeed * Time.deltaTime
            );
        }
    }
}