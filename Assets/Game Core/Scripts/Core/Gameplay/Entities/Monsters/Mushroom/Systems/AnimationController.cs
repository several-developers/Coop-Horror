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
            _animator = mushroomEntity.GetAnimator();
            _agent = mushroomEntity.GetAgent();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.AnimationConfig _animationConfig;
        private readonly MushroomReferences _references;
        private readonly Animator _animator;
        private readonly NavMeshAgent _agent;

        private Tweener _hatTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            float deltaTime = Time.deltaTime;
            float happiness = _mushroomEntity.Happiness;
            float isSneaking = _mushroomEntity.IsSneaking;
            float dampTime = _animationConfig.DampTime;

            _animator.SetFloat(id: AnimatorHashes.Happiness, value: happiness, dampTime, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.IsSneaking, value: isSneaking, dampTime, deltaTime);
            
            UpdateAnimationMoveSpeed();
        }

        public void ChangeEmotion(MushroomEntity.Emotion emotion)
        {
        }

        public void ChangeHatState(bool isHatDamaged)
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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateAnimationMoveSpeed()
        {
            float value = MonstersUtilities.GetAgentClampedSpeed(_agent);
            _animator.SetFloat(id: AnimatorHashes.MoveSpeed, value);
        }
    }
}