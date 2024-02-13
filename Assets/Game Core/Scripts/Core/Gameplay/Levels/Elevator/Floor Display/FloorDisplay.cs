﻿using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class FloorDisplay : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshPro _floorNumberTMP;
        
        [SerializeField, Required]
        private Transform _moveDirectionTransform;

        // FIELDS: --------------------------------------------------------------------------------

        private ElevatorManager _elevatorManager;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            UpdateFloorNumber(ElevatorFloor.Surface);
            
            _elevatorManager = ElevatorManager.Get();
            
            _elevatorManager.OnElevatorStartedEvent += OnElevatorStarted;
            _elevatorManager.OnFloorChangedEvent += OnFloorChanged;
        }

        private void OnDestroy()
        {
            _elevatorManager.OnElevatorStartedEvent -= OnElevatorStarted;
            _elevatorManager.OnFloorChangedEvent -= OnFloorChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateDisplayInfo(ElevatorStaticData data)
        {
            ElevatorFloor currentFloor = data.CurrentFloor;
            bool isTargetFloor = data.IsTargetFloor;
            
            _moveDirectionTransform.gameObject.SetActive(!isTargetFloor);
            
            UpdateFloorNumber(currentFloor);
            UpdateMoveDirection(data.IsMovingUp);
        }

        private void UpdateFloorNumber(ElevatorFloor floor)
        {
            string text = floor switch
            {
                ElevatorFloor.One => "1",
                ElevatorFloor.Two => "2",
                ElevatorFloor.Three => "3",
                _ => "~"
            };

            _floorNumberTMP.text = text;
        }

        private void UpdateMoveDirection(bool isMovingUp)
        {
            Vector3 rotation = isMovingUp ? Vector3.zero : new Vector3(x: 0f, y: 0f, z: 180f);
            _moveDirectionTransform.rotation = Quaternion.Euler(rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnElevatorStarted(ElevatorStaticData data) => UpdateDisplayInfo(data);

        private void OnFloorChanged(ElevatorStaticData data) => UpdateDisplayInfo(data);
    }
}