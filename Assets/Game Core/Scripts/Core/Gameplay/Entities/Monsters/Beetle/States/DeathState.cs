using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(BeetleEntity beetleEntity) =>
            _beetleEntity = beetleEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            DisableAggressionSystem();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            agent.enabled = false;
        }
        
        private void DisableAggressionSystem()
        {
            AggressionSystem aggressionSystem = _beetleEntity.GetAggressionSystem();
            aggressionSystem.Disable();
        }
    }
}