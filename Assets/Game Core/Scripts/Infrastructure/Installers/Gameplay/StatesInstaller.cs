using GameCore.Infrastructure.StateMachine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class StatesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindPrepareGameplaySceneState();
            BindGameplaySceneState();
            BindQuitGameplaySceneState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindPrepareGameplaySceneState()
        {
            Container
                .Bind<PrepareGameplaySceneState>()
                .AsSingle()
                .NonLazy();
        }
        
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
    }
}