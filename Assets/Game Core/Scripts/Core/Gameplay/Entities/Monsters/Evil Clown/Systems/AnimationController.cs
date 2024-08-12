using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
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
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _agent = evilClownEntity.GetAgent();
            _transform = evilClownEntity.transform;
            _animator = animator;
            _checkAnimationRoutine = new CoroutineHelper(evilClownEntity);

            EvilClownAIConfigMeta evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _animationConfig = evilClownAIConfig.AnimationConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly EvilClownAIConfigMeta.AnimationSettings _animationConfig;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly Animator _animator;
        private readonly CoroutineHelper _checkAnimationRoutine;

        private float _runningType;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartAnimationCheck()
        {
            _checkAnimationRoutine.GetRoutineEvent += CheckAnimationCO;
            
            _checkAnimationRoutine.Start();
        }

        public void Tick()
        {
            float dampTime = _animationConfig.TypeChangeDuration;
            float deltaTime = Time.deltaTime;
            float moveSpeedMultiplier = GetAnimationMoveSpeedMultiplier();
            
            _animator.SetFloat(id: AnimatorHashes.RunningType, value: _runningType, dampTime, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.MoveSpeedMultiplier, value: moveSpeedMultiplier, dampTime, deltaTime);
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

            _runningType = runningType;
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
        
        private float GetAnimationMoveSpeedMultiplier()
        {
            float maxMoveSpeed = _evilClownAIConfig.MaxChaseSpeed;
            bool isZero = Mathf.Approximately(a: maxMoveSpeed, b: 0f);

            if (isZero)
                return 1f;

            float currentMoveSpeed = _agent.velocity.magnitude;
            float percent = currentMoveSpeed / maxMoveSpeed;

            Vector2 range = _animationConfig.AnimationSpeedMultiplierRange;
            float multiplier = Mathf.Lerp(a: 0f, b: range.y, t: percent);
            float result = Mathf.Clamp(value: multiplier, min: range.x, max: range.y);
            
            return result;
        }
    }
}