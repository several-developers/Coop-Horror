using System;
using GameCore.Gameplay.HorrorStateMachineSpace;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class HorrorStateMachineInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindHorrorStateMachine();
            BindPrepareGameState();
            BindGameLoopState();
            BindLoadLocationState();
            BindLeaveLocationState();
            BindQuitState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindHorrorStateMachine()
        {
            Container
                .BindInterfacesTo<HorrorStateMachine>()
                .AsSingle();
        }

        private void BindPrepareGameState()
        {
            Container
                .Bind(typeof(PrepareGameState), typeof(IDisposable))
                .To<PrepareGameState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindGameLoopState()
        {
            Container
                .Bind<GameLoopState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindLoadLocationState()
        {
            Container
                .Bind(typeof(LoadLocationState), typeof(IDisposable))
                .To<LoadLocationState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindLeaveLocationState()
        {
            Container
                .Bind<LeaveLocationState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindQuitState()
        {
            Container
                .Bind<QuitState>()
                .AsSingle()
                .NonLazy();
        }
    }
}