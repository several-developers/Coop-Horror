using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Factories.ItemsPreview;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindItemsPreviewFactory();
            BindItemsFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindItemsPreviewFactory()
        {
            Container
                .BindInterfacesTo<ItemsPreviewFactory>()
                .AsSingle();
        }

        private void BindItemsFactory()
        {
            Container
                .BindInterfacesTo<ItemsFactory>()
                .AsSingle();
        }
    }
}