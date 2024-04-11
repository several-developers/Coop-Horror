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
            BindRpcHandlerDecorator();
            BindElevatorsManagerDecorator();
            BindQuestsManagerDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
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
    }
}