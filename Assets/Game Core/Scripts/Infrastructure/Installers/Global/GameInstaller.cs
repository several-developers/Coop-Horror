using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Observers;
using GameCore.Infrastructure.StateMachine;
using GameCore.Utilities;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class GameInstaller : MonoInstaller, ICoroutineRunner
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindCoroutineRunner();
            BindGameStateMachine();
            BindMenuFactory();
            BindGameStateMachineObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindCoroutineRunner()
        {
            Container
                .Bind<ICoroutineRunner>()
                .FromInstance(this)
                .AsSingle()
                .NonLazy();
        }

        private void BindGameStateMachine()
        {
            Container
                .BindInterfacesTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();
        }

        private void BindMenuFactory()
        {
            Container
                .Bind<MenuFactory>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameStateMachineObserver()
        {
            Container
                .BindInterfacesTo<GameStateMachineObserver>()
                .AsSingle()
                .NonLazy();
        }
    }
}