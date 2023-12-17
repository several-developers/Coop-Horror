using GameCore.Gameplay.Entities.Other;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Zombies.States
{
    public class ScreamState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScreamState(ZombieEntity zombieEntity)
        {
            _zombieEntity = zombieEntity;
            _animationObserver = zombieEntity.GetAnimationObserver();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ZombieEntity _zombieEntity;
        private readonly AnimationObserver _animationObserver;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {

            TriggerScream();
        }

        public void Exit()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TriggerScream()
        {
            Animator animator = _zombieEntity.GetAnimator();
            animator.SetTrigger(AnimatorHashes.Scream);
        }
        
        private void EnterChaseState() =>
            _zombieEntity.EnterChaseState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnScreamFinished() => EnterChaseState();
    }
}