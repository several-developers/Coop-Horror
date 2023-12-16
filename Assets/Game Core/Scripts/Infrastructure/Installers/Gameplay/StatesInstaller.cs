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
        
        private void BindGameOverState()
        {
            Container
                .Bind<GameOverState>()
                .AsSingle()
                .NonLazy();
        }
    }
}