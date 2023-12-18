using GameCore.Gameplay.Observers;
using GameCore.Gameplay.Observers.UI;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
            BindGraphyStateObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle();
        }

        private void BindGraphyStateObserver()
        {
            Container
                .BindInterfacesTo<GraphyStateObserver>()
                .AsSingle();
        }
    }
}