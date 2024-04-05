using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Level;
using GameCore.Observers.Gameplay.LevelManager;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.Rpc;
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
            BindLevelObserver();
            BindLevelProviderObserver();
            BindRpcObserver();
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

        private void BindLevelObserver()
        {
            Container
                .BindInterfacesTo<LevelObserver>()
                .AsSingle();
        }

        private void BindLevelProviderObserver()
        {
            Container
                .BindInterfacesTo<LevelProviderObserver>()
                .AsSingle();
        }

        private void BindRpcObserver()
        {
            Container
                .BindInterfacesTo<RpcObserver>()
                .AsSingle();
        }
    }
}