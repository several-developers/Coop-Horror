using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Elevator;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.UI;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class ObserversInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
            BindPlayerInteractionObserver();
            BindDungeonsObserver();
            BindElevatorObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle();
        }

        private void BindPlayerInteractionObserver()
        {
            Container
                .BindInterfacesTo<PlayerInteractionObserver>()
                .AsSingle();
        }

        private void BindDungeonsObserver()
        {
            Container
                .BindInterfacesTo<DungeonsObserver>()
                .AsSingle();
        }

        private void BindElevatorObserver()
        {
            Container
                .BindInterfacesTo<ElevatorObserver>()
                .AsSingle();
        }
    }
}