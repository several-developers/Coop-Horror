using System;
using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class AnimationController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AnimationController(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();

            _mushroomEntity = mushroomEntity;
            _animationConfig = mushroomAIConfig.GetAnimationConfig();
            _references = mushroomEntity.GetReferences();
            _agent = mushroomEntity.GetAgent();
            _animator = _references.Animator;
            _modelTransform = _references.ModelTransform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnHideCompletedEvent = delegate { }; 

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.AnimationConfig _animationConfig;
        private readonly MushroomReferences _references;
        private readonly NavMeshAgent _agent;
        private readonly Animator _animator;
        private readonly Transform _modelTransform;

        private Tweener _hatTN;
        private Tweener _sittingTN;
        private bool _isHiding;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            float deltaTime = Time.deltaTime;
            float happiness = _mushroomEntity.IsHatDamaged ? 0f : 1f;
            float isSneaking = _mushroomEntity.IsSneaking ? 1f : 0f;
            float dampTime = _animationConfig.DampTime;

            _animator.SetFloat(id: AnimatorHashes.Happiness, value: happiness, dampTime, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.IsSneaking, value: isSneaking, dampTime, deltaTime);
            
            UpdateAnimationMoveSpeed();
            UpdateAnimationMultipliers();
        }

        public void SetEmotion(MushroomEntity.Emotion emotion)
        {
            SkinnedMeshRenderer eyes = _references.Eyes;
            SkinnedMeshRenderer mouth = _references.Mouth;
            
            switch (emotion)
            {
                case MushroomEntity.Emotion.Regular:
                    break;
                
                case MushroomEntity.Emotion.Happy:
                    eyes.SetBlendShapeWeight(index: 0, value: 0);
                    mouth.SetBlendShapeWeight(index: 0, value: 0);
                    break;
                
                case MushroomEntity.Emotion.Angry:
                    eyes.SetBlendShapeWeight(index: 0, value: 100);
                    mouth.SetBlendShapeWeight(index: 0, value: 100);
                    break;
                
                case MushroomEntity.Emotion.Scared:
                    break;
                
                case MushroomEntity.Emotion.Interested:
                    break;
                
                case MushroomEntity.Emotion.Sigma:
                    break;
                
                case MushroomEntity.Emotion.Dead:
                    break;
            }
        }

        public void SetHatState(bool isHatDamaged)
        {
            GameObject hatSpores = _references.HatSpores;
            SkinnedMeshRenderer hat = _references.Hat;

            if (isHatDamaged)
                hatSpores.SetActive(false);

            float from = hat.GetBlendShapeWeight(index: 0);
            float to = isHatDamaged ? 100f : 0f;

            float duration = isHatDamaged
                ? _animationConfig.HatExplosionDuration
                : _animationConfig.HatRegenerationDuration;

            Ease ease = isHatDamaged
                ? _animationConfig.HatExplosionEase
                : _animationConfig.HatRegenerationEase;

            _hatTN.Kill();

            _hatTN = DOVirtual
                .Float(from, to, duration, onVirtualUpdate: t =>
                {
                    hat.SetBlendShapeWeight(index: 0, value: t);
                })
                .SetLink(_mushroomEntity.gameObject)
                .SetEase(ease);
        }

        public void SetHidingState(bool isHiding)
        {
            _isHiding = isHiding;
            _animator.SetBool(id: AnimatorHashes.IsHiding, isHiding);

            ChangeModelHidingState(isHiding);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateAnimationMoveSpeed()
        {
            float value = _isHiding 
                ? 0f
                : MonstersUtilities.GetAgentClampedSpeed(_agent);
            
            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value);
        }

        private void UpdateAnimationMultipliers()
        {
            float sitDownMultiplier = _animationConfig.SitDownAnimationMultiplier;
            float standUpMultiplier = _animationConfig.StandUpAnimationMultiplier;
            
            _animator.SetFloat(id: AnimatorHashes.SitDownSpeedMultiplier, sitDownMultiplier);
            _animator.SetFloat(id: AnimatorHashes.StandUpSpeedMultiplier, standUpMultiplier);
        }

        private void ChangeModelHidingState(bool isHiding)
        {
            float endValue = isHiding ? _animationConfig.ModelSittingY : 0f;
            float duration = isHiding ? _animationConfig.ModelSitDownDuration : _animationConfig.ModelStandUpDuration;
            float delay = isHiding ? _animationConfig.ModelSitDownDelay : _animationConfig.ModelStandUpDelay;
            Ease ease = isHiding ? _animationConfig.ModelSitDownEase : _animationConfig.ModelStandUpEase;
            
            _sittingTN.Kill();

            _sittingTN = _modelTransform
                .DOLocalMoveY(endValue, duration)
                .SetEase(ease)
                .SetDelay(delay)
                .SetLink(_mushroomEntity.gameObject)
                .OnStepComplete(() =>
                {
                    OnHideCompletedEvent.Invoke();
                });
        }
    }
}