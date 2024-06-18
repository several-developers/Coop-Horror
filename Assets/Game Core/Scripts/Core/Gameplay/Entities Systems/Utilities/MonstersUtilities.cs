using System.Collections.Generic;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Utilities
{
    public static class MonstersUtilities
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static bool TryGetClosestAlivePlayer(Vector3 worldPosition, out PlayerEntity result)
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            float minDistance = float.MaxValue;
            bool isPlayerFound = false;

            foreach (KeyValuePair<ulong, PlayerEntity> pair in allPlayers)
            {
                PlayerEntity playerEntity = pair.Value;
                bool isDead = playerEntity.IsDead();
                
                if (isDead)
                    continue;

                Vector3 playerPosition = playerEntity.transform.position;
                float distance = Vector3.Distance(a: worldPosition, b: playerPosition);
                
                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                result = playerEntity;
                isPlayerFound = true;
            }

            result = null;
            return isPlayerFound;
        }
    }
}