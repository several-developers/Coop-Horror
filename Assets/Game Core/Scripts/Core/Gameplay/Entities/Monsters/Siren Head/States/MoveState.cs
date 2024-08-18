using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.GameTimeManagement;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SirenHead.States
{
    public class MoveState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveState(SirenHeadEntity sirenHeadEntity, ITimeCycle timeCycle)
        {
            _sirenHeadEntity = sirenHeadEntity;
            _references = sirenHeadEntity.GetReferences();
            _sirenHeadAIConfig = sirenHeadEntity.GetAIConfig();
            _timeCycle = timeCycle;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly SirenHeadEntity _sirenHeadEntity;
        private readonly SirenHeadEntity.References _references;
        private readonly SirenHeadAIConfigMeta _sirenHeadAIConfig;
        private readonly ITimeCycle _timeCycle;

        private Tweener _movementTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            StartMovement();
            ToggleAnimationMovement(canMove: true);
        }

        public void Exit()
        {
            StopMovement();
            ToggleAnimationMovement(canMove: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleAnimationMovement(bool canMove)
        {
            Animator animator = _references.Animator;
            float moveSpeed = canMove ? 1f : 0f;
            animator.SetFloat(id: AnimatorHashes.MoveSpeed, moveSpeed);
        }
        
        private void StartMovement()
        {
            StopMovement();

            Transform transform = _sirenHeadEntity.transform;
            Transform targetPoint = _references.TargetPoint;
            Vector3 targetPosition = targetPoint.position;

            float duration = CalculateMoveTime();
            
            _movementTN = transform
                .DOMove(targetPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(EnterIdleState);
        }

        private void StopMovement() =>
            _movementTN.Kill();

        private float CalculateMoveTime()
        {
            const int minutesInDay = Constants.MinutesInDay;
            int currentTimeInMinutes = _timeCycle.GetCurrentTimeInMinutes();
            int arriveTimeOffset = _sirenHeadAIConfig.ArriveTimeOffset;
            int arriveTime = minutesInDay + arriveTimeOffset;

            int timeLeftInMinutes = arriveTime - currentTimeInMinutes;
            float minuteDurationInSeconds = _timeCycle.GetMinuteDurationInSeconds();

            float moveTime = timeLeftInMinutes * minuteDurationInSeconds;

            // string log = Log.HandleLog($"Current Time in Minutes: <gb>{currentTimeInMinutes}</gb>,   \n" +
            //                            $"Arrive Time: <gb>{arriveTime}</gb>,   \n" +
            //                            $"Time Left: <gb>{timeLeftInMinutes}</gb>,   \n" +
            //                            $"Move Time: <gb>{moveTime}</gb>,   \n" +
            //                            $"Minute Duration in Seconds: <gb>{minuteDurationInSeconds}</gb>");
            //
            // Debug.Log(log);
            
            return moveTime;
        }
        
        private void EnterIdleState() =>
            _sirenHeadEntity.EnterIdleState();
    }
}