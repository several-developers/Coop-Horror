using GameCore.Infrastructure.StateMachine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class StatesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameplayState();
            BindQuitGameplayState();
            BindGameOverState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindGameplayState()
        {
            Container
                .Bind<GameplayState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindQuitGameplayState()
        {
            Container
                .Bind<QuitGameplayState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindGameOverState()
        {
            Container
                .Bind<GameOverState>()
                .AsSingle()
                .NonLazy();
        }
    }
}