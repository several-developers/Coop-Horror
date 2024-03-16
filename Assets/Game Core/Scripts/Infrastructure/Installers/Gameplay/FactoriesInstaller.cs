using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.Factories.Player;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindPlayerFactory();
            BindItemsPreviewFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindPlayerFactory()
        {
            Container
                .BindInterfacesTo<PlayerFactory>()
                .AsSingle();
        }

        private void BindItemsPreviewFactory()
        {
            Container
                .BindInterfacesTo<ItemsPreviewFactory>()
                .AsSingle();
        }
    }
}