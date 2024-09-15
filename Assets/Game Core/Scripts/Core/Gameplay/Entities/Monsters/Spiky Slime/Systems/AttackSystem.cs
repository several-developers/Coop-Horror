using System.Collections;
using DG.Tweening;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class AttackSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackSystem(SpikySlimeEntity spikySlimeEntity, AggressionSystem aggressionSystem)
        {
            SpikySlimeAIConfigMeta spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();

            _spikySlimeEntity = spikySlimeEntity;
            _attackSystemConfig = spikySlimeAIConfig.GetAttackSystemConfig();
            _references = spikySlimeEntity.GetReferences();
            _aggressionSystem = aggressionSystem;
            _attackRoutine = new CoroutineHelper(spikySlimeEntity);

            _attackRoutine.GetRoutineEvent += AttackCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.AttackSystemConfig _attackSystemConfig;
        private readonly SpikySlimeReferences _references;
        private readonly AggressionSystem _aggressionSystem;
        private readonly CoroutineHelper _attackRoutine;

        private Tweener _spikesTN;
        private bool _attackInProgress;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartAttack()
        {
            if (_attackInProgress)
                return;

            _attackInProgress = true;
            PlayAttackAnimation();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayAttackAnimation()
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
                .OnComplete(OnAttack);
        }

        private void PlayHideSpikesAnimation()
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
                .OnComplete(OnAttackFinished);
        }

        private void KillAllAround()
        {
            
        }

        private IEnumerator AttackCO()
        {
            KillAllAround();
            
            float spikesDuration = _attackSystemConfig.SpikesDuration;
            yield return new WaitForSeconds(spikesDuration);

            PlayHideSpikesAnimation();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAttack() =>
            _attackRoutine.Start();

        private void OnAttackFinished()
        {
            _attackInProgress = false;
            _aggressionSystem.StartDecreaseTimer();
        }
    }
}