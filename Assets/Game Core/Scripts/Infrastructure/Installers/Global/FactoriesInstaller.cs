using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Gameplay.Factories.Monsters;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMenuFactory();
            BindEntitiesFactory();
            BindMonstersFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindMenuFactory()
        {
            Container
                .BindInterfacesTo<MenuFactory>()
                .AsSingle();
        }
        
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
    }
}