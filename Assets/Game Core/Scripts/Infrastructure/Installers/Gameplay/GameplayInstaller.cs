using GameCore.Gameplay.Locations.GameTime;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.UI;
using GameCore.Observers.Global.Graphy;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class GameplayInstaller : MonoInstaller
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Sun _sun;
        
        [SerializeField, Required]
        private GameTimeSystem _gameTimeSystem;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindUIObserver();
            BindPlayerInteractionObserver();
            BindGraphyStateObserver();
            BindSun();
            BindGameTimeSystem();
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

        private void BindGraphyStateObserver()
        {
            Container
                .BindInterfacesTo<GraphyStateObserver>()
                .AsSingle();
        }

        private void BindSun()
        {
            Container
                .Bind<Sun>()
                .FromInstance(_sun)
                .AsSingle();
        }

        private void BindGameTimeSystem()
        {
            Container
                .BindInterfacesTo<TimeCycle>()
                .AsSingle();
        }
    }
}