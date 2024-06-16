using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.Factories.Monsters;
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
            BindItemsFactory();
            BindMonstersFactory();
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

        private void BindItemsFactory()
        {
            Container
                .BindInterfacesTo<ItemsFactory>()
                .AsSingle();
        }

        private void BindMonstersFactory()
        {
            Container
                .BindInterfacesTo<MonstersFactory>()
                .AsSingle();
        }
    }
}