using UnityEngine;

namespace GameCore.Gameplay.Entities.Zombies.States
{
    public class IdleState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(ZombieEntity zombieEntity) =>
            _zombieEntity = zombieEntity;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ZombieEntity _zombieEntity;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => SetIdleAnimation();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetIdleAnimation()
        {
            Animator animator = _zombieEntity.GetAnimator();
            animator.SetFloat(id: AnimatorHashes.Speed, value: 0f);
        }
    }
}