using System.Collections;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class SearchForTargetState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SearchForTargetState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _agent = goodClownEntity.GetAgent();
            _transform = goodClownEntity.transform;
            _searchTargetRoutine = new CoroutineHelper(goodClownEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float SearchInterval = 1f;

        private readonly GoodClownEntity _goodClownEntity;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly CoroutineHelper _searchTargetRoutine;

        private float _cachedAgentSpeed;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _searchTargetRoutine.GetRoutineEvent += SearchTargetCO;

            DisableAgent();
            StartHunterSystem();
            _searchTargetRoutine.Start();
        }

        public void Exit()
        {
            _searchTargetRoutine.GetRoutineEvent -= SearchTargetCO;

            ResetAgent();
            _searchTargetRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            _cachedAgentSpeed = _agent.speed;
            _agent.speed = 0f;
        }

        private void ResetAgent() =>
            _agent.speed = _cachedAgentSpeed;

        private void StartHunterSystem()
        {
            HunterSystem hunterSystem = _goodClownEntity.GetHunterSystem();
            hunterSystem.Start();
        }

        private void EnterFollowTargetState() =>
            _goodClownEntity.EnterFollowTargetState();

        private IEnumerator SearchTargetCO()
        {
            while (true)
            {
                yield return new WaitForSeconds(SearchInterval);
                
                bool isTargetFound = TryFoundTarget(out PlayerEntity targetPlayer);

                // string log = Log.HandleLog($"Is Target Found: <gb>{isTargetFound}</gb>");
                // Debug.Log(log);

                if (!isTargetFound)
                    continue;

                _goodClownEntity.ToggleInnocent(false);
                _goodClownEntity.SetTargetPlayer(targetPlayer);
                EnterFollowTargetState();
                break;
            }
        }

        private bool TryFoundTarget(out PlayerEntity result)
        {
            Vector3 clownPosition = _transform.position;
            EntityLocation clownLocation = _goodClownEntity.EntityLocation;
            Floor clownFloor = _goodClownEntity.CurrentFloor;
            bool isInnocent = _goodClownEntity.IsInnocent;

            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            PlayerEntity closestPlayer = null;
            float minDistance = float.MaxValue;
            bool isPlayerFound = false;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                EntityLocation playerLocation = playerEntity.EntityLocation;
                Floor playerFloor = playerEntity.CurrentFloor;
                bool isPlayerPositionValid = true;

                if (isInnocent)
                {
                    if (clownLocation == EntityLocation.Dungeon)
                    {
                        isPlayerPositionValid = playerLocation == EntityLocation.Dungeon &&
                                                playerFloor == clownFloor;
                    }
                }
                else
                {
                    if (clownLocation == EntityLocation.Dungeon)
                    {
                        isPlayerPositionValid = playerLocation
                            is EntityLocation.Dungeon
                            or EntityLocation.Stairs;
                    }
                }
                
                if (!isPlayerPositionValid)
                    continue;

                Vector3 playerPosition = playerEntity.transform.position;
                float distance = Vector3.Distance(a: clownPosition, b: playerPosition);
                
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