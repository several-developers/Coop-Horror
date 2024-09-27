using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SirenHead.States
{
    public class IdleState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(SirenHeadEntity sirenHeadEntity) =>
            _sirenHeadEntity = sirenHeadEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly SirenHeadEntity _sirenHeadEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => StopAnimationMovement();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopAnimationMovement()
        {
            SirenHeadEntity.References references = _sirenHeadEntity.GetReferences();
            Animator animator = references.Animator;
            animator.SetFloat(id: AnimatorHashes.MoveSpeed, value: 0f, dampTime: 2f, Time.deltaTime);
        }
    }
}