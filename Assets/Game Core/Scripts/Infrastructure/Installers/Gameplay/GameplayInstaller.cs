using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Observers;
using GameCore.Gameplay.Observers.UI;
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
        private PlayerEntity _playerEntity;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindPlayerEntity();
            BindUIObserver();
            BindGraphyStateObserver();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        private void BindGraphyStateObserver()
        {
            Container
                .BindInterfacesTo<GraphyStateObserver>()
                .AsSingle();
        }
    }
}