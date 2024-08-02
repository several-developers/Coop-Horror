using GameCore.Gameplay.Network;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(GoodClownEntity goodClownEntity) =>
            _goodClownEntity = goodClownEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            DisableHunterSystem();
            ResetBalloon();
            KillSelf();
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
            hunterSystem.StopHuntingTimer();
        }

        private void ResetBalloon() => 
            _goodClownEntity.ResetBalloonServerRpc();

        private void KillSelf()
        {
            if (!NetworkHorror.IsTrueServer)
                return;
            
            Object.Destroy(_goodClownEntity.gameObject);
        }
    }
}