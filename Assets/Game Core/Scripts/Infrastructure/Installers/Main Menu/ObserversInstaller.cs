using GameCore.Observers.MainMenu.UI;
using Zenject;

namespace GameCore.Infrastructure.Installers.MainMenu
{
    public class ObserversInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle();
        }
    }
}