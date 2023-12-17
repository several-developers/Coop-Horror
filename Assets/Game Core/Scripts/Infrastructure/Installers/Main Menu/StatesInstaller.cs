using GameCore.Infrastructure.StateMachine;
using Zenject;

namespace GameCore.Infrastructure.Installers.MainMenu
{
    public class StatesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindPrepareMainMenuState();
            BindOnlineMenuState();
            BindOfflineMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindPrepareMainMenuState()
        {
            Container
                .Bind<PrepareMainMenuState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindOnlineMenuState()
        {
            Container
                .Bind<OnlineMenuState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindOfflineMenuState()
        {
            Container
                .Bind<OfflineMenuState>()
                .AsSingle()
                .NonLazy();
        }
    }
}