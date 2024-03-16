using GameCore.Infrastructure.StateMachine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class StatesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameplaySceneState();
            BindQuitGameplaySceneState();
            BindGameOverState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindGameplaySceneState()
        {
            Container
                .Bind<GameplaySceneState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindQuitGameplaySceneState()
        {
            Container
                .Bind<QuitGameplaySceneState>()
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