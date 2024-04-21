using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class ProvidersInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameplayConfigsProvider();
            BindItemsMetaProvider();
            BindItemsProvider();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindGameplayConfigsProvider()
        {
            Container
                .BindInterfacesTo<GameplayConfigsProvider>()
                .AsSingle();
        }
        
        private void BindItemsMetaProvider()
        {
            Container
                .BindInterfacesTo<ItemsMetaProvider>()
                .AsSingle()
                .NonLazy();
        }
        
        private void BindItemsProvider()
        {
            Container
                .BindInterfacesTo<ItemsProvider>()
                .AsSingle();
        }
    }
}