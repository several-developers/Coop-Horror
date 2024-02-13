using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Other
{
    public class NetworkServiceLocator : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IHorrorStateMachine horrorStateMachine, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _horrorStateMachine = horrorStateMachine;
            _gameplayConfigsProvider = gameplayConfigsProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkServiceLocator _instance;

        private IHorrorStateMachine _horrorStateMachine;
        private IGameplayConfigsProvider _gameplayConfigsProvider;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IHorrorStateMachine GetHorrorStateMachine() => _horrorStateMachine;

        public IGameplayConfigsProvider GetGameplayConfigsProvider() => _gameplayConfigsProvider;

        public static NetworkServiceLocator Get() => _instance;
    }
}