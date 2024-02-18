using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public abstract class ElevatorBase : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------

        private ElevatorManager _elevatorManager;
        private RpcCaller _rpcCaller;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Start()
        {
            _elevatorManager = ElevatorManager.Get();
            _rpcCaller = RpcCaller.Get();

            _elevatorManager.OnElevatorStartedEvent += OnElevatorStarted;
            _elevatorManager.OnFloorChangedEvent += OnFloorChanged;

            _rpcCaller.OnOpenElevatorEvent += OnOpenElevator;
        }

        private void OnDestroy()
        {
            _elevatorManager.OnElevatorStartedEvent -= OnElevatorStarted;
            _elevatorManager.OnFloorChangedEvent -= OnFloorChanged;

            _rpcCaller.OnOpenElevatorEvent -= OnOpenElevator;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ChangeElevatorFloor(Floor floor) =>
            _floor = floor;

        public Floor GetElevatorFloor() => _floor;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OpenElevator()
        {
            if (_isOpen)
                return;
            
            _isOpen = true;
            _animator.SetTrigger(id: AnimatorHashes.Open);
        }

        private void CloseElevator()
        {
            _isOpen = false;
            _animator.SetTrigger(id: AnimatorHashes.Close);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnElevatorStarted(ElevatorStaticData data)
        {
            bool isSameFloor = data.CurrentFloor == _floor;

            if (!isSameFloor)
                return;
            
            CloseElevator();
        }

        private void OnFloorChanged(ElevatorStaticData data)
        {
            bool isTargetFloor = data.IsTargetFloor;
            
            if (!isTargetFloor)
                return;
            
            Floor currentFloor = data.CurrentFloor;
            bool isSameFloor = currentFloor == _floor;

            if (!isSameFloor)
                return;
            
            OpenElevator();
        }

        private void OnOpenElevator(Floor floor)
        {
            bool isSameFloor = floor == _floor;

            if (!isSameFloor)
                return;
            
            OpenElevator();
        }
    }
}