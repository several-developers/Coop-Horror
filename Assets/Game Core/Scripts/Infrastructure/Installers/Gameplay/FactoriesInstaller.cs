using GameCore.Gameplay.Factories.Player;
using GameCore.Gameplay.Other;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindPlayerFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindPlayerFactory()
        {
            if (GameStaticState.IsMultiplayerEnabled)
            {
                Container
                    .BindInterfacesTo<PlayerNetworkFactory>()
                    .AsSingle();
            }
            else
            {
                Container
                   .BindInterfacesTo<PlayerFactory>()
                   .AsSingle();
            }
        }
    }
}