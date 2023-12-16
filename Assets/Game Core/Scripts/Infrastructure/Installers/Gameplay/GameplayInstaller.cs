using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Observers;
using GameCore.Gameplay.Observers.Taps;
using GameCore.Gameplay.Observers.UI;
using GameCore.Gameplay.Other.MainCamera;
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
        private MainCamera _mainCamera;

        [SerializeField, Required]
        private PlayerEntity _playerEntity;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindMainCamera();
            BindPlayerEntity();
            BindUIObserver();
            BindTapsObserver();
            BindGraphyStateObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindMainCamera()
        {
            Container
                .Bind<IMainCamera>()
                .FromInstance(_mainCamera)
                .AsSingle();
        }

        private void BindPlayerEntity()
        {
            Container
                .Bind<IPlayerEntity>()
                .FromInstance(_playerEntity)
                .AsSingle();
        }

        private void BindUIObserver()
        {
            Container
                .BindInterfacesTo<UIObserver>()
                .AsSingle();
        }

        private void BindTapsObserver()
        {
            Container
                .BindInterfacesTo<TapsObserver>()
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