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
            BindLeaveLocationServerState();
            BindLeaveLocationClientState();
            BindGenerateDungeonsState();
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
                .Bind<PrepareGameState>()
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
                .Bind<LoadLocationState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindLeaveLocationServerState()
        {
            Container
                .Bind(typeof(LeaveLocationServerState), typeof(IDisposable))
                .To<LeaveLocationServerState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindLeaveLocationClientState()
        {
            Container
                .Bind<LeaveLocationClientState>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindGenerateDungeonsState()
        {
            Container
                .Bind<GenerateDungeonsState>()
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