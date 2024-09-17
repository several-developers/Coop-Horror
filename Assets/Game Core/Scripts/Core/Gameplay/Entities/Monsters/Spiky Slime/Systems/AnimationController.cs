using System;
using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class AnimationController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AnimationController(SpikySlimeEntity spikySlimeEntity)
        {
            SpikySlimeAIConfigMeta spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();

            _spikySlimeEntity = spikySlimeEntity;
            _aggressionSystemConfig = spikySlimeAIConfig.GetAggressionSystemConfig();
            _attackSystemConfig = spikySlimeAIConfig.GetAttackSystemConfig();
            _animationConfig = spikySlimeAIConfig.GetAnimationConfig();
            _references = spikySlimeEntity.GetReferences();
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAttackAnimationEndedEvent = delegate { };
        public event Action OnHideSpikesAnimationEndedEvent = delegate { };
        
        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.AggressionSystemConfig _aggressionSystemConfig;
        private readonly SpikySlimeAIConfigMeta.AttackSystemConfig _attackSystemConfig;
        private readonly SpikySlimeAIConfigMeta.AnimationConfig _animationConfig;
        private readonly SpikySlimeReferences _references;

        private Tweener _aggressiveAnimationTN;
        private Tweener _spikesTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateAggressionAnimation(int aggressionMeter)
        {
            int maxMeter = _aggressionSystemConfig.AggressionMeterToAggro;
            float percent = aggressionMeter / (float)maxMeter;

            float calmMultiplier = _animationConfig.CalmMultiplier;
            float angryMultiplier = _animationConfig.AngryMultiplier;
            float targetMultiplier = Mathf.Lerp(a: calmMultiplier, b: angryMultiplier, t: percent);
            
            float calmAnimationSpeed = _animationConfig.CalmAnimationSpeed;
            float angryAnimationSpeed = _animationConfig.AngryAnimationSpeed;
            float targetAnimationSpeed = Mathf.Lerp(a: calmAnimationSpeed, b: angryAnimationSpeed, t: percent);

            float duration = _animationConfig.AnimationDuration;
            Ease ease = _animationConfig.AnimationEase;
            
            SkinnedMeshSine skinnedMeshSine = _references.SkinnedMeshSine;

            float startMultiplier = skinnedMeshSine.PeakMultiplier;
            float startAnimationSpeed = skinnedMeshSine.AnimationSpeed;
            
            _aggressiveAnimationTN.Kill();

            _aggressiveAnimationTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float multiplier = Mathf.Lerp(a: startMultiplier, b: targetMultiplier, t);
                    float animationSpeed = Mathf.Lerp(a: startAnimationSpeed, b: targetAnimationSpeed, t);

                    skinnedMeshSine.PeakMultiplier = multiplier;
                    skinnedMeshSine.AnimationSpeed = animationSpeed;
                })
                .SetEase(ease)
                .SetLink(_spikySlimeEntity.gameObject);
        }

        public void PlayAttackAnimation()
        {
            SkinnedMeshRenderer slimeRenderer = _references.SlimeRenderer;
            float duration = _attackSystemConfig.ShowSpikesAnimationDuration;
            Ease ease = _attackSystemConfig.ShowSpikesAnimationEase;

            _spikesTN.Kill();

            _spikesTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float shapeValue = Mathf.Lerp(a: 0f, b: 100f, t);
                    slimeRenderer.SetBlendShapeWeight(index: 0, shapeValue);
                })
                .SetEase(ease)
                .SetLink(_spikySlimeEntity.gameObject)
                .OnComplete(() => OnAttackAnimationEndedEvent.Invoke());
        }
        
        public void PlayHideSpikesAnimation()
        {
            SkinnedMeshRenderer slimeRenderer = _references.SlimeRenderer;
            float duration = _attackSystemConfig.HideSpikesAnimationDuration;
            Ease ease = _attackSystemConfig.HideSpikesAnimationEase;

            _spikesTN.Kill();

            _spikesTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float shapeValue = Mathf.Lerp(a: 100f, b: 0f, t);
                    slimeRenderer.SetBlendShapeWeight(index: 0, shapeValue);
                })
                .SetEase(ease)
                .SetLink(_spikySlimeEntity.gameObject)
                .OnComplete(() => OnHideSpikesAnimationEndedEvent.Invoke());
        }
    }
}