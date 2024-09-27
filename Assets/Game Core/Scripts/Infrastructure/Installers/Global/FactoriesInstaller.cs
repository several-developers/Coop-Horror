using GameCore.Gameplay.Factories.Menu;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMenusFactory();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindMenusFactory()
        {
            Container
                .BindInterfacesTo<MenusFactory>()
                .AsSingle();
        }
    }
}