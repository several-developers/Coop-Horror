using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class LookAtInterestTargetState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LookAtInterestTargetState(MushroomEntity mushroomEntity)
        {
            _mushroomEntity = mushroomEntity;
            _whisperingSystem = mushroomEntity.GetWhisperingSystem();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly WhisperingSystem _whisperingSystem;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _mushroomEntity.DisableAgent();
            SetSneakingState(isSneaking: true);
            UnpauseWhisperingSystem();
            LookAtTarget();
        }

        public void Exit() => SetSneakingState(isSneaking: false);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetSneakingState(bool isSneaking) =>
            _mushroomEntity.SetSneakingState(isSneaking);

        private void UnpauseWhisperingSystem() =>
            _whisperingSystem.Unpause();

#warning TEMP, make smoother
        private void LookAtTarget()
        {
            PlayerEntity interestTarget = _mushroomEntity.GetInterestTarget();
            
            if (interestTarget == null)
                return;

            Transform transform = _mushroomEntity.transform;
            Vector3 targetPosition = interestTarget.transform.position;

            transform.LookAt(targetPosition);
        }
    }
}