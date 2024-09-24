using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Level.Elevator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public class CurrentFloorDisplay : FloorDisplayBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Required]
        private Transform _moveDirectionTransform;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();
            UpdateFloorNumber();
            
            ElevatorEntity.OnCurrentFloorChangedEvent += OnCurrentFloorChanged;
        }

        private void OnDestroy()
        {
            ElevatorEntity.OnCurrentFloorChangedEvent -= OnCurrentFloorChanged;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void UpdateFloorNumber() => UpdateDisplayInfo();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void UpdateDisplayInfo()
        {
            Floor currentFloor = ElevatorEntity.GetCurrentFloor();
            Floor targetFloor = ElevatorEntity.GetTargetFloor();
            bool isTargetFloor = currentFloor == targetFloor;
            
            _moveDirectionTransform.gameObject.SetActive(!isTargetFloor);
            
            UpdateFloorNumber(currentFloor);
            UpdateMoveDirection();
        }

        private void UpdateMoveDirection()
        {
            ElevatorEntity.ElevatorState elevatorState = ElevatorEntity.GetElevatorState();
            bool isMovingUp = elevatorState == ElevatorEntity.ElevatorState.MovingUp;
            Vector3 rotation = isMovingUp ? Vector3.zero : new Vector3(x: 0f, y: 0f, z: 180f);
            _moveDirectionTransform.rotation = Quaternion.Euler(rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCurrentFloorChanged(Floor floor) => UpdateFloorNumber();
    }
}