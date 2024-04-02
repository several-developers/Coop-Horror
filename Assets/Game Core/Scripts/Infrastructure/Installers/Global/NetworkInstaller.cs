using GameCore.Enums.Global;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.UnityServices.Auth;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.Lifecycle;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class NetworkInstaller : MonoInstaller
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private NetcodeHooks _netcodeHooksPrefab;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindNetworkHorrorDecorator();
            BindNetcodeHooks();
            
            
            Test();
            BindMessageChannels();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindNetworkHorrorDecorator()
        {
            Container
                .BindInterfacesTo<NetworkHorrorDecorator>()
                .AsSingle();
        }

        private void BindNetcodeHooks()
        {
            Container
                .Bind<INetcodeHooks>()
                .To<NetcodeHooks>()
                .FromComponentInNewPrefab(_netcodeHooksPrefab)
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
                .Bind<ApplicationController>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
            
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
            
            Container
                .BindInterfacesTo<MessageChannel<QuitApplicationMessage>>()
                .AsSingle();
            
            // Buffered message channels hold the latest received message in buffer and pass to any new subscribers.
            Container
                .BindInterfacesTo<BufferedMessageChannel<LobbyListFetchedMessage>>()
                .AsSingle();
        }
    }
}