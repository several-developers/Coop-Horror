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
            BindPlayModeSelectionMenu();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindPrepareMainMenuState()
        {
            Container
                .Bind<PrepareMainMenuState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindPlayModeSelectionMenu()
        {
            Container
                .Bind<PlayModeSelectionMenu>()
                .AsSingle()
                .NonLazy();
        }
    }
}