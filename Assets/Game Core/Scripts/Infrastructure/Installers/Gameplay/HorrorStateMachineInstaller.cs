using GameCore.Gameplay.HorrorStateMachineSpace;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class HorrorStateMachineInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindHorrorStateMachine();
            BindPrepareGameState();
            BindGameLoopState();
            BindQuitState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindHorrorStateMachine()
        {
            Container
                .BindInterfacesTo<HorrorStateMachine>()
                .AsSingle();
        }

        private void BindPrepareGameState()
        {
            Container
                .Bind<PrepareGameState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindGameLoopState()
        {
            Container
                .Bind<GameLoopState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindQuitState()
        {
            Container
                .Bind<QuitState>()
                .AsSingle()
                .NonLazy();
        }
    }
}