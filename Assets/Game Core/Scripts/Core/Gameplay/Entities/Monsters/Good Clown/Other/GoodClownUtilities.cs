using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class GoodClownUtilities
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GoodClownUtilities(GoodClownEntity goodClownEntity, Animator animator)
        {
            _goodClownEntity = goodClownEntity;
            _agent = goodClownEntity.GetAgent();
            _animator = animator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownEntity _goodClownEntity;
        private readonly NavMeshAgent _agent;
        private readonly Animator _animator;

        private Vector2 _smoothDeltaPosition;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetIdleAnimation() =>
            _animator.SetBool(id: AnimatorHashes.IsWalking, value: false);

        public void SetWalkingAnimation() =>
            _animator.SetBool(id: AnimatorHashes.IsWalking, value: true);

        public void UpdateAnimationMoveSpeed()
        {
            float value = GetCurrentClampedSpeed();
            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private float GetCurrentClampedSpeed()
        {
            float targetSpeed = _agent.speed;

            if (Mathf.Approximately(a: targetSpeed, b: 0f))
                return 0f;

            float agentSpeed = _agent.velocity.magnitude;
            return agentSpeed / targetSpeed;
        }
    }
}