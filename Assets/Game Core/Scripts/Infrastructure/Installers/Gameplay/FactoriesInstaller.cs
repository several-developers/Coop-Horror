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
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindItemsPreviewFactory()
        {
            Container
                .BindInterfacesTo<ItemsPreviewFactory>()
                .AsSingle();
        }
    }
}