using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class PrepareToChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareToChaseState(EvilClownEntity evilClownEntity, Rig rig)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
            _rig = rig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float RigAnimationDuration = 1f;
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly WanderingTimer _wanderingTimer;
        private readonly Rig _rig;
        
        private Coroutine _brainwashSFXLoopCO;
        private Tweener _rigTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            PlaySound(EvilClownEntity.SFXType.Roar);
            EnableRig();
            StopWanderingTimer();
            EnterChaseStateWithDelay().Forget();
            StopBrainwashSFXLoop();
        }

        public void Exit() => StartBrainwashSFXLoop();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableRig()
        {
            _rigTN.Kill();

            _rigTN = DOVirtual
                .Float(from: 0f, to: 1f, RigAnimationDuration, onVirtualUpdate: t =>
                {
                    _rig.weight = t;
                })
                .SetLink(_evilClownEntity.gameObject);
        }
        
        private void StopWanderingTimer() =>
            _wanderingTimer.StopTimer();
        
        private void StartBrainwashSFXLoop()
        {
            IEnumerator routine = BrainwashSFXLoopCO();
            _evilClownEntity.StartCoroutine(routine);
        }

        private void StopBrainwashSFXLoop()
        {
            if (_brainwashSFXLoopCO == null)
                return;
            
            _evilClownEntity.StopCoroutine(_brainwashSFXLoopCO);
        }

        private void PlaySound(EvilClownEntity.SFXType sfxType) =>
            _evilClownEntity.PlaySound(sfxType).Forget();

        private async UniTaskVoid EnterChaseStateWithDelay()
        {
            float delayInSeconds = _evilClownAIConfig.ChaseDelay;
            int delay = delayInSeconds.ConvertToMilliseconds();
            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _evilClownEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            EnterChaseState();
        }

        private void EnterChaseState() =>
            _evilClownEntity.EnterChaseState();
        
        private IEnumerator BrainwashSFXLoopCO()
        {
            while (true)
            {
                Vector2 range = _evilClownAIConfig.BrainWashSoundsDelay;
                float delay = Random.Range(range.x, range.y);

                yield return new WaitForSeconds(delay);
                
                PlaySound(EvilClownEntity.SFXType.Brainwash);
            }
        }
    }
}