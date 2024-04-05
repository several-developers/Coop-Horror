using GameCore.Gameplay.Levels.Elevator;
using GameCore.Gameplay.Network;
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
    }
}