using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Utilities
{
    public static class MonstersUtilities
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static bool TryGetClosestAlivePlayer(
            Vector3 worldPosition,
            EntityLocation location,
            out PlayerEntity result
        )
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            PlayerEntity closestPlayer = null;
            float minDistance = float.MaxValue;
            bool isPlayerFound = false;

            foreach (KeyValuePair<ulong, PlayerEntity> pair in allPlayers)
            {
                PlayerEntity playerEntity = pair.Value;
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                EntityLocation playerLocation = playerEntity.EntityLocation;
                bool isLocationMatches = playerLocation == location;

                if (!isLocationMatches)
                    continue;
                
                Vector3 playerPosition = playerEntity.transform.position;
                float distance = Vector3.Distance(a: worldPosition, b: playerPosition);

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestPlayer = playerEntity;
                isPlayerFound = true;
            }

            result = closestPlayer;
            return isPlayerFound;
        }
        
        public static bool TryGetClosestAlivePlayer(Vector3 worldPosition, out PlayerEntity result)
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            PlayerEntity closestPlayer = null;
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
                closestPlayer = playerEntity;
                isPlayerFound = true;
            }

            result = closestPlayer;
            return isPlayerFound;
        }
    }
}