using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Levels;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Other
{
    public class NetworkServiceLocator : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IHorrorStateMachine horrorStateMachine, ILevelProvider levelProvider)
        {
            _horrorStateMachine = horrorStateMachine;
            _levelProvider = levelProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkServiceLocator _instance;

        private IHorrorStateMachine _horrorStateMachine;
        private ILevelProvider _levelProvider;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IHorrorStateMachine GetHorrorStateMachine() => _horrorStateMachine;

        public ILevelProvider GetLevelManager() => _levelProvider;

        public static NetworkServiceLocator Get() => _instance;
    }
}