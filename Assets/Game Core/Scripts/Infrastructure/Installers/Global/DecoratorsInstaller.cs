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
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindCollectionsServiceDecorator()
        {
            Container
                .BindInterfacesTo<CollectionsServiceDecorator>()
                .AsSingle();
        }
    }
}