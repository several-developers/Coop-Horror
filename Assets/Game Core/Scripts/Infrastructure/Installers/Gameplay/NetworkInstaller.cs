using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class NetworkInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameManagerDecorator();
            BindRpcHandlerDecorator();
            BindElevatorsManagerDecorator();
            BindQuestsManagerDecorator();
            BindGameTimeManagerDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindGameManagerDecorator()
        {
            Container
                .BindInterfacesTo<GameManagerDecorator>()
                .AsSingle();
        }
        
        private void BindRpcHandlerDecorator()
        {
            Container
                .BindInterfacesTo<RpcHandlerDecorator>()
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
    }
}