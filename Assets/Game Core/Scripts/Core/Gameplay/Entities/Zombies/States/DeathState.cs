using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Zombies.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(ZombieEntity zombieEntity, CapsuleCollider aimTarget)
        {
            _zombieEntity = zombieEntity;
            _aimTarget = aimTarget;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float DestroyDelay = 20f;
        
        private readonly ZombieEntity _zombieEntity;
        private readonly CapsuleCollider _aimTarget;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAnimationMovement();
            DisableAgent();
            DisableHitBoxes();
            EnabledRagdoll();
            DestroyZombie();
            SendDeathEvent();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAnimationMovement()
        {
            Animator animator = _zombieEntity.GetAnimator();
            animator.SetBool(id: AnimatorHashes.CanMove, value: false);
            animator.SetFloat(id: AnimatorHashes.Speed, value: 0f);
        }
        
        private void DisableAgent()
        {
            NavMeshAgent agent = _zombieEntity.GetAgent();
            agent.enabled = false;
        }

        private void DisableHitBoxes()
        {
            _aimTarget.enabled = false;
            _zombieEntity.DisableHitBoxes();
        }

        private void EnabledRagdoll()
        {
            //_ragdollEnabler.EnableRagdoll();
        }

        private void DestroyZombie() =>
            Object.Destroy(_zombieEntity.gameObject, DestroyDelay);

        private void SendDeathEvent() =>
            _zombieEntity.SendDeathEvent();
    }
}