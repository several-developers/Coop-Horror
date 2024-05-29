using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.Providers.Gameplay.RigPresets;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class ProvidersInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindGameplayConfigsProvider();
            BindMonstersAIConfigsProvider();
            BindItemsMetaProvider();
            BindItemsProvider();
            BindRigPresetsProvider();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindGameplayConfigsProvider()
        {
            Container
                .BindInterfacesTo<GameplayConfigsProvider>()
                .AsSingle();
        }
        
        private void BindMonstersAIConfigsProvider()
        {
            Container
                .BindInterfacesTo<MonstersAIConfigsProvider>()
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
        
        private void BindRigPresetsProvider()
        {
            Container
                .BindInterfacesTo<RigPresetsProvider>()
                .AsSingle();
        }
    }
}