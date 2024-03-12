using GameCore.Enums.Global;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.UnityServices.Auth;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.StateMachine;
using GameCore.Observers.Global.StateMachine;
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

            Test();
            BindMessageChannels();
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

        private void Test()
        {
            Container
                .Bind<LocalLobby>()
                .AsSingle();

            Container
                .Bind<LocalLobbyUser>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<LobbyServiceFacade>()
                .AsSingle();

            Container
                .Bind<AuthenticationServiceFacade>()
                .AsSingle();

            Container
                .Bind<ProfileManager>()
                .AsSingle();

            // var connectionManager =
            //     Container.InstantiateComponentOnNewGameObject<ConnectionManager>("Connection Manager");
            //
            // Container
            //     .Bind<ConnectionManager>()
            //     .AsSingle();
            
            Container
                .Bind<ConnectionManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
        }

        private void BindMessageChannels()
        {
            // These message channels are essential and persist for the lifetime of the lobby and relay services.
            Container
                .BindInterfacesTo<MessageChannel<ConnectStatus>>()
                .AsSingle();
            
            Container
                .BindInterfacesTo<MessageChannel<ReconnectMessage>>()
                .AsSingle();
            
            Container
                .BindInterfacesTo<MessageChannel<ConnectionEventMessage>>()
                .AsSingle();
            
            // Buffered message channels hold the latest received message in buffer and pass to any new subscribers.
            Container
                .BindInterfacesTo<BufferedMessageChannel<LobbyListFetchedMessage>>()
                .AsSingle();
        }
    }
}