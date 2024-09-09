using GameCore.Gameplay.Factories;
using GameCore.Observers.Global.StateMachine;
using GameCore.StateMachine;
using GameCore.Utilities;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class GameInstaller : MonoInstaller, ICoroutineRunner
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindCoroutineRunner();
            BindUpdateRunner();
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

        private void BindUpdateRunner()
        {
            GameObject updateRunnerObject = new(name: "Update Runner");
            var updateRunner = updateRunnerObject.AddComponent<UpdateRunner>();

            Container
                .Bind<IUpdateRunner>()
                .FromInstance(updateRunner)
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
                .Bind<MenuStaticFactory>()
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