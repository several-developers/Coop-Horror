using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Utilities;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class PrepareToChaseState : IEnterState
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

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            StopWanderingTimer();
            EnterChaseStateWithDelay();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopWanderingTimer() =>
            _wanderingTimer.StopTimer();

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
    }
}