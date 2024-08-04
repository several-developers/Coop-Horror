using System.Collections;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class PrepareToChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareToChaseState(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly WanderingTimer _wanderingTimer;
        
        private Coroutine _brainwashSFXLoopCO;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            StopWanderingTimer();
            EnterChaseStateWithDelay();
            StopBrainwashSFXLoop();
            _evilClownEntity.PlaySound(EvilClownEntity.SFXType.Roar);
        }

        public void Exit() => StartBrainwashSFXLoop();

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        private async void EnterChaseStateWithDelay()
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
                
                _evilClownEntity.PlaySound(EvilClownEntity.SFXType.Brainwash);
            }
        }
    }
}