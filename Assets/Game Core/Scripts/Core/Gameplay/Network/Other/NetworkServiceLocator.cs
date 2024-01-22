using GameCore.Gameplay.HorrorStateMachineSpace;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Other
{
    public class NetworkServiceLocator : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IHorrorStateMachine horrorStateMachine)
        {
            _horrorStateMachine = horrorStateMachine;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkServiceLocator _instance;
        
        private IHorrorStateMachine _horrorStateMachine;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public IHorrorStateMachine GetHorrorStateMachine() => _horrorStateMachine;
        
        public static NetworkServiceLocator Get() => _instance;
    }
}