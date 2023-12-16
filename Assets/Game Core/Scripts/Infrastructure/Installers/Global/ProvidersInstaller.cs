using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.Data;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class ProvidersInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindDataProvider();
            BindAssetsProvider();
            BindConfigsProvider();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindDataProvider()
        {
            Container
                .BindInterfacesTo<DataProvider>()
                .AsSingle()
                .NonLazy();
        }

        private void BindAssetsProvider()
        {
            Container
                .BindInterfacesTo<AssetsProvider>()
                .AsSingle()
                .NonLazy();
        }

        private void BindConfigsProvider()
        {
            Container
                .BindInterfacesTo<ConfigsProvider>()
                .AsSingle()
                .NonLazy();
        }
    }
}