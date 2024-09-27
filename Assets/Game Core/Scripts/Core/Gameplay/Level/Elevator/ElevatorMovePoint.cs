using GameCore.Enums.Gameplay;
using GameCore.Observers.Gameplay.LevelManager;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ElevatorMovePoint : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProviderObserver levelProviderObserver) =>
            _levelProviderObserver = levelProviderObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _movePointFloor;
        
        // FIELDS: --------------------------------------------------------------------------------

        private ILevelProviderObserver _levelProviderObserver;

        private void Awake() =>
            _levelProviderObserver.RegisterElevatorMovePoint(this);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Floor GetFloor() => _movePointFloor;
    }
}