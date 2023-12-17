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
    }
}