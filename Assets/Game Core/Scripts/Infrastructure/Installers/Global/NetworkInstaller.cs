using GameCore.Gameplay.Network;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class NetworkInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindNetworkHorrorDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindNetworkHorrorDecorator()
        {
            Container
                .BindInterfacesTo<NetworkHorrorDecorator>()
                .AsSingle();
        }
    }
}