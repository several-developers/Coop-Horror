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
            BindAssetsWarmUpState();
            BindLoadAssetsWarmUpSceneState();
            BindLoadMainMenuSceneState();
            BindLoadGameplaySceneState();

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

        private void BindAssetsWarmUpState()
        {
            Container
                .Bind<AssetsWarmUpState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadAssetsWarmUpSceneState()
        {
            Container
                .Bind<LoadAssetsWarmUpSceneState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadMainMenuSceneState()
        {
            Container
                .Bind<LoadMainMenuSceneState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindLoadGameplaySceneState()
        {
            Container
                .Bind<LoadGameplaySceneState>()
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