using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Infrastructure.Services.Global.Decorators.CollectionsService;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class DecoratorsInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindCollectionsServiceDecorator();
            BindDynamicPrefabsLoaderDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindCollectionsServiceDecorator()
        {
            Container
                .BindInterfacesTo<CollectionsServiceDecorator>()
                .AsSingle();
        }
        
        private void BindDynamicPrefabsLoaderDecorator()
        {
            Container
                .BindInterfacesTo<DynamicPrefabsLoaderDecorator>()
                .AsSingle();
        }
    }
}