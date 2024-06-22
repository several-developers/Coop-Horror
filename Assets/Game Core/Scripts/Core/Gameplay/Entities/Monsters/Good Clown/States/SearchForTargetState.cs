using System.Collections;
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
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _agent = goodClownEntity.GetAgent();
            _transform = goodClownEntity.transform;
            _searchLogicRoutine = new CoroutineHelper(goodClownEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float SearchInterval = 1f;

        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly CoroutineHelper _searchLogicRoutine;

        private float _cachedAgentSpeed;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _searchLogicRoutine.GetRoutineEvent += SearchLogicCO;
            
            DisableAgent();
            SetIdleAnimation();
            StartHunterSystem();
            _searchLogicRoutine.Start();
        }

        public void Exit()
        {
            _searchLogicRoutine.GetRoutineEvent -= SearchLogicCO;
            
            ResetAgent();
            _searchLogicRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            _cachedAgentSpeed = _agent.speed;
            _agent.speed = 0f;
        }

        private void ResetAgent() =>
            _agent.speed = _cachedAgentSpeed;

        private void SetIdleAnimation() =>
            _clownUtilities.SetIdleAnimation();

        private void StartHunterSystem()
        {
            HunterSystem hunterSystem = _goodClownEntity.GetHunterSystem();
            hunterSystem.Start();
        }

        private void EnterFollowTargetState() =>
            _goodClownEntity.EnterFollowTargetState();

        private IEnumerator SearchLogicCO()
        {
            while (true)
            {
                yield return new WaitForSeconds(SearchInterval);

                Vector3 position = _transform.position;

                //bool isTargetFound = MonstersUtilities.TryGetClosestAlivePlayer(position, EntityLocation.Dungeon,
                //out PlayerEntity targetPlayer);

                bool isTargetFound =
                    MonstersUtilities.TryGetClosestAlivePlayer(position, out PlayerEntity targetPlayer);

                if (!isTargetFound)
                    continue;

                _goodClownEntity.SetTargetPlayer(targetPlayer);
                EnterFollowTargetState();
                break;
            }
        }
    }
}