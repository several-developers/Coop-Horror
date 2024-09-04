using GameCore.Gameplay.Factories.Menu;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class FactoriesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings() => BindMenuFactory();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindMenuFactory()
        {
            Container
                .BindInterfacesTo<MenuFactory>()
                .AsSingle();
        }
    }
}