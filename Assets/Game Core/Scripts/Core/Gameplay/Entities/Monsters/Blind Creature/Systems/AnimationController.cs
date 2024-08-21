using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class AnimationController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AnimationController(BlindCreatureEntity blindCreatureEntity)
        {
            BlindCreatureEntity.References references = blindCreatureEntity.GetReferences();
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();

            _blindCreatureEntity = blindCreatureEntity;
            _animationConfig = blindCreatureAIConfig.GetAnimationConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _agent = blindCreatureEntity.GetAgent();
            _animator = references.CreatureAnimator;
            _networkAnimator = references.NetworkAnimator;
            _modelPivot = references.ModelPivot;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.AnimationConfig _animationConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly NavMeshAgent _agent;
        private readonly Animator _animator;
        private readonly NetworkAnimator _networkAnimator;
        private readonly Transform _modelPivot;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start() => LevitateCycle(moveUp: true);

        public void Tick()
        {
            // TEMP
            UpdateAnimation();
        }

        public void PlayAttackAnimation() =>
            _networkAnimator.SetTrigger(hash: AnimatorHashes.Attack);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateAnimation()
        {
            float moveSpeed = MonstersUtilities.GetAgentClampedSpeed(_agent);
            float dampValue = _animationConfig.DampTime;
            float deltaTime = Time.deltaTime;

            bool isAggressive = _suspicionSystem.IsAggressive();
            float aggressiveness = isAggressive ? 1f : 0f;

            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value: moveSpeed, dampValue, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.Aggressiveness, value: aggressiveness, dampValue, deltaTime);
        }

        private void LevitateCycle(bool moveUp)
        {
            Vector2 levitatingRange = _animationConfig.LevitatingRange;
            Vector2 levitatingDuration = _animationConfig.LevitatingDuration;

            float endValue = moveUp ? levitatingRange.y : levitatingRange.x;
            float duration = Random.Range(levitatingDuration.x, levitatingDuration.y);
            Ease ease = _animationConfig.LevitatingEase;

            _modelPivot
                .DOLocalMoveY(endValue, duration)
                .SetEase(ease)
                .SetLink(_blindCreatureEntity.gameObject)
                .OnComplete(() => LevitateCycle(!moveUp));
        }
    }
}