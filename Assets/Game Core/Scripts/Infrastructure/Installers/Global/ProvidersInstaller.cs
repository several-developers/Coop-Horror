using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.Data;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
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
            BindItemsMetaProvider();
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
        
        private void BindItemsMetaProvider()
        {
            Container
                .BindInterfacesTo<ItemsMetaProvider>()
                .AsSingle()
                .NonLazy();
        }
    }
}