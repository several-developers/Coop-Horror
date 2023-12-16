using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class ProvidersInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameplayConfigsProvider();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindGameplayConfigsProvider()
        {
            Container
                .BindInterfacesTo<GameplayConfigsProvider>()
                .AsSingle();
        }
    }
}