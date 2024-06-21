using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class RespawnAsEvilClownState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public RespawnAsEvilClownState(GoodClownEntity goodClownEntity) =>
            _goodClownEntity = goodClownEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            DisableHunterSystem();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _goodClownEntity.GetAgent();
            agent.enabled = false;
        }
        
        private void DisableHunterSystem()
        {
            HunterSystem hunterSystem = _goodClownEntity.GetHunterSystem();
            hunterSystem.Stop();
        }
    }
}