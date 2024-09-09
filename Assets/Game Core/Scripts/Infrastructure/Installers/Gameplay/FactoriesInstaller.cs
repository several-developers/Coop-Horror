using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.Factories.Monsters;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindEntitiesFactory();
            BindMonstersFactory();
            BindItemsFactory();
            BindItemsPreviewFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindEntitiesFactory()
        {
            Container
                .BindInterfacesTo<EntitiesFactory>()
                .AsSingle();
        }

        private void BindMonstersFactory()
        {
            Container
                .BindInterfacesTo<MonstersFactory>()
                .AsSingle();
        }

        private void BindItemsFactory()
        {
            Container
                .BindInterfacesTo<ItemsFactory>()
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