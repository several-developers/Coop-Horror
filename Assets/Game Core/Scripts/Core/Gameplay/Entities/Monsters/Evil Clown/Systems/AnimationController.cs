using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown
{
    public class AnimationController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AnimationController(EvilClownEntity evilClownEntity, Animator animator)
        {
            _evilClownEntity = evilClownEntity;
            _agent = evilClownEntity.GetAgent();
            _transform = evilClownEntity.transform;
            _animator = animator;
            _checkAnimationRoutine = new CoroutineHelper(evilClownEntity);

            EvilClownAIConfigMeta evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _animationConfig = evilClownAIConfig.AnimationConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta.AnimationSettings _animationConfig;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly Animator _animator;
        private readonly CoroutineHelper _checkAnimationRoutine;

        private float _previousRunningType = -1f;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartAnimationCheck()
        {
            _checkAnimationRoutine.GetRoutineEvent += CheckAnimationCO;
            
            _checkAnimationRoutine.Start();
        }

        public void StopAnimationCheck()
        {
            _checkAnimationRoutine.GetRoutineEvent -= CheckAnimationCO;
            
            _checkAnimationRoutine.Stop();
        }

        public void UpdateAnimationMoveSpeed()
        {
            float value = MonstersUtilities.GetAgentClampedSpeed(_agent);
            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckAnimation()
        {
            PlayerEntity targetPlayer = _evilClownEntity.GetTargetPlayer();
            bool isTargetValid = targetPlayer != null && !targetPlayer.IsDead();

            if (!isTargetValid)
                return;

            Vector3 playerPosition = targetPlayer.transform.position;
            Vector3 clownPosition = _transform.position;
            float distance = Vector3.Distance(a: clownPosition, b: playerPosition);
            float runningType = 0f;

            float typeOneDistance = _animationConfig.RunningFirstTypeDistance;
            float typeTwoDistance = _animationConfig.RunningSecondTypeDistance;

            if (distance < typeOneDistance && distance >= typeTwoDistance)
                runningType = 1f;
            else if (distance < typeTwoDistance)
                runningType = 2f;

            bool isSameRunningType = Mathf.Approximately(a: runningType, b: _previousRunningType);
            _previousRunningType = runningType;

            if (!isSameRunningType)
                return;
            
            float dampTime = _animationConfig.TypeChangeDuration;
            float deltaTime = Time.deltaTime;
            
            _animator.SetFloat(id: AnimatorHashes.RunningType, value: runningType, dampTime, deltaTime);
        }

        private IEnumerator CheckAnimationCO()
        {
            while (true)
            {
                float checkInterval = _animationConfig.DistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);

                CheckAnimation();
            }
        }
    }
}