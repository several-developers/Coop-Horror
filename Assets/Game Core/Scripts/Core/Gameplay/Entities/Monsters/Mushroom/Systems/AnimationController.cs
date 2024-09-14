using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using GameCore.Utilities;
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

        private const int AngryIndex = 0;
        private const int FearIndex = 1;
        private const int DeathIndex = 3;
        private const int SurpriseIndex = 4;

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
            SkinnedMeshRenderer sigmaFace = _references.SigmaFace;

            bool showSigmaFace = false;

            float angryEyesValue = 0f;
            float angryMouthValue = 0f;
            float fearEyesValue = 0f;
            float surpriseEyesValue = 0f;
            
            float fearMouthValue = 0f;
            float deathEyesValue = 0f;
            float deathMouthValue = 0f;
            float surpriseMouthValue = 0f;
            
            bool changeBlendShape = true;
            
            switch (emotion)
            {
                case MushroomEntity.Emotion.Regular:
                    angryMouthValue = 30f;
                    break;
                
                case MushroomEntity.Emotion.Happy:
                    // Default values.
                    break;
                
                case MushroomEntity.Emotion.Angry:
                    angryEyesValue = 100f;
                    angryMouthValue = 100f;
                    break;
                
                case MushroomEntity.Emotion.Scared:
                    fearEyesValue = 100f;
                    fearMouthValue = 60f;
                    break;
                
                case MushroomEntity.Emotion.Interested:
                    surpriseEyesValue = 70f;
                    surpriseMouthValue = 50f;
                    break;
                
                case MushroomEntity.Emotion.Sigma:
                    changeBlendShape = false;
                    showSigmaFace = true;
                    break;
                
                case MushroomEntity.Emotion.Dead:
                    deathEyesValue = 100f;
                    deathMouthValue = 100f;
                    break;
            }

            eyes.enabled = !showSigmaFace;
            mouth.enabled = !showSigmaFace;
            sigmaFace.enabled = showSigmaFace;

            if (!changeBlendShape)
                return;
            
            eyes.SetBlendShapeWeight(AngryIndex, angryEyesValue);
            eyes.SetBlendShapeWeight(FearIndex, fearEyesValue);
            eyes.SetBlendShapeWeight(DeathIndex, deathEyesValue);
            eyes.SetBlendShapeWeight(SurpriseIndex, surpriseEyesValue);
            
            mouth.SetBlendShapeWeight(AngryIndex, angryMouthValue);
            mouth.SetBlendShapeWeight(FearIndex, fearMouthValue);
            mouth.SetBlendShapeWeight(DeathIndex, deathMouthValue);
            mouth.SetBlendShapeWeight(SurpriseIndex, surpriseMouthValue);
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

        public void SetHidingState(bool isHiding, bool instant = false)
        {
            _isHiding = isHiding;
            _animator.SetBool(id: AnimatorHashes.IsHiding, isHiding);

            ChangeModelHidingState(isHiding, instant);
        }

        public async UniTaskVoid ResetSigmaFaceAfterDelay()
        {
            float delayInSeconds = _animationConfig.SigmaFaceResetDelay;
            int delay = delayInSeconds.ConvertToMilliseconds();
            
            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _mushroomEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            MushroomEntity.Emotion emotion = _mushroomEntity.IsHatDamaged
                ? MushroomEntity.Emotion.Angry
                : MushroomEntity.Emotion.Happy;
            
            _mushroomEntity.SetEmotion(emotion);
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

        private void ChangeModelHidingState(bool isHiding, bool instant = false)
        {
            float endValue = isHiding ? _animationConfig.ModelSittingY : 0f;
            float duration = isHiding ? _animationConfig.ModelSitDownDuration : _animationConfig.ModelStandUpDuration;
            float delay = isHiding ? _animationConfig.ModelSitDownDelay : _animationConfig.ModelStandUpDelay;
            Ease ease = isHiding ? _animationConfig.ModelSitDownEase : _animationConfig.ModelStandUpEase;

            if (instant)
            {
                duration = 0f;
                delay = 0f;
            }
            
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