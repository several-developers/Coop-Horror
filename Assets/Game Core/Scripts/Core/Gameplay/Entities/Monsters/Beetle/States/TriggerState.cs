using GameCore.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class TriggerState : IEnterState, ITickableState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TriggerState(BeetleEntity beetleEntity)
        {
            _beetleEntity = beetleEntity;
            _transform = beetleEntity.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly Transform _transform;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            EnableAggressionSystemTriggerCheck();
        }

        public void Tick() => LookAtTarget();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            agent.enabled = false;
        }

        private void EnableAggressionSystemTriggerCheck()
        {
            AggressionSystem aggressionSystem = _beetleEntity.GetAggressionSystem();
            aggressionSystem.ToggleTriggerCheckState(isEnabled: true);
        }

        private void LookAtTarget()
        {
            PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();
            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 beetlePosition = _transform.position;
            Vector3 lookVector = targetPosition - beetlePosition;
            
            _transform.LookAt(targetPosition);
        }
    }
}