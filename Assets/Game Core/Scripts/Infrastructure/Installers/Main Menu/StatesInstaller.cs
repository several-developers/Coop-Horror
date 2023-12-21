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
            BindCreateLobbyState();
            BindJoinGameState();
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

        private void BindCreateLobbyState()
        {
            Container
                .Bind<CreateLobbyState>()
                .AsSingle()
                .NonLazy();
        }

        private void BindJoinGameState()
        {
            Container
                .Bind<JoinGameState>()
                .AsSingle()
                .NonLazy();
        }
    }
}