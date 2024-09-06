using GameCore.Gameplay.ChatManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Gameplay.Systems.Quests;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class NetworkInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameManagerDecorator();
            BindElevatorsManagerDecorator();
            BindQuestsManagerDecorator();
            BindGameTimeManagerDecorator();
            BindChatManagerDecorator();
            BindNetworkMessages();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindGameManagerDecorator()
        {
            Container
                .BindInterfacesTo<GameManagerDecorator>()
                .AsSingle();
        }

        private void BindElevatorsManagerDecorator()
        {
            Container
                .BindInterfacesTo<ElevatorsManagerDecorator>()
                .AsSingle();
        }
        
        private void BindQuestsManagerDecorator()
        {
            Container
                .BindInterfacesTo<QuestsManagerDecorator>()
                .AsSingle();
        }
        
        private void BindGameTimeManagerDecorator()
        {
            Container
                .BindInterfacesTo<GameTimeManagerDecorator>()
                .AsSingle();
        }

        private void BindChatManagerDecorator()
        {
            Container
                .BindInterfacesTo<ChatManagerDecorator>()
                .AsSingle();
        }

        private void BindNetworkMessages()
        {
            Container
                .BindInterfacesTo<NetworkedMessageChannel<UIEventMessage>>()
                .AsSingle();
            
            Container
                .BindInterfacesTo<NetworkedMessageChannel<GenerateDungeonsMessage>>()
                .AsSingle();
        }
    }
}