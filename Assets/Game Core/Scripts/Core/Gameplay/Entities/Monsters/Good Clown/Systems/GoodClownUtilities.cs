using GameCore.Gameplay.Systems.Utilities;
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
        
        public void SetAgonizeAnimation() =>
            _animator.SetBool(id: AnimatorHashes.IsAgonizing, value: true);

        public void UpdateAnimationMoveSpeed()
        {
            float value = MonstersUtilities.GetAgentClampedSpeed(_agent);
            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value);
        }
    }
}