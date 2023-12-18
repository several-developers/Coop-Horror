using GameCore.Infrastructure.StateMachine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class StatesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindBootstrapState();
            BindLoadDataState();
            BindLoadMainMenuState();
            BindLoadGameplayState();

#if UNITY_EDITOR
            BindGameSetupForEditorState();
#endif
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindBootstrapState()
        {
            Container
                .Bind<BootstrapState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadDataState()
        {
            Container
                .Bind<LoadDataState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadMainMenuState()
        {
            Container
                .Bind<LoadMainMenuState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadGameplayState()
        {
            Container
                .Bind<LoadGameplayState>()
                .AsSingle()
                .NonLazy();
        }

#if UNITY_EDITOR
        private void BindGameSetupForEditorState()
        {
            Container
                .Bind<GameSetupForEditorState>()
                .AsSingle()
                .NonLazy();
        }
#endif
    }
}