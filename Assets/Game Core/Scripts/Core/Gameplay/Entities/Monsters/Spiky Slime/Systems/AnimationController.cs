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
            AggressionSystem aggressionSystem = spikySlimeEntity.GetAggressionSystem();
            
            _spikySlimeEntity = spikySlimeEntity;
            _aggressionSystemConfig = spikySlimeAIConfig.GetAggressionSystemConfig();
            _animationConfig = spikySlimeAIConfig.GetAnimationConfig();
            _references = spikySlimeEntity.GetReferences();

            aggressionSystem.OnAggressionMeterChangedEvent += OnAggressionMeterChanged;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.AggressionSystemConfig _aggressionSystemConfig;
        private readonly SpikySlimeAIConfigMeta.AnimationConfig _animationConfig;
        private readonly SpikySlimeReferences _references;

        private Tweener _spikesAnimationTN;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleAnimation(int aggressionMeter)
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
            
            _spikesAnimationTN.Kill();

            _spikesAnimationTN = DOVirtual
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAggressionMeterChanged(int aggressionMeter) => HandleAnimation(aggressionMeter);
    }
}