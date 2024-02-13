using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class ElevatorTeleportTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawTrigger;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private ElevatorBase _elevator;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<PlayerEntity> _playersList = new();
        
        private ILevelManager _levelManager;
        private ElevatorManager _elevatorManager;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            GetLevelManager();
            
            _elevatorManager = ElevatorManager.Get();
            _elevatorManager.OnFloorChangedEvent += OnFloorChanged;
        }

        private void OnDestroy() =>
            _elevatorManager.OnFloorChangedEvent -= OnFloorChanged;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerEntity playerEntity))
                _playersList.Add(playerEntity);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerEntity playerEntity))
                _playersList.Remove(playerEntity);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool DrawTrigger() => _drawTrigger;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GetLevelManager()
        {
            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            _levelManager = networkServiceLocator.GetLevelManager();
        }
        
        private void TryTeleportLocalPlayer()
        {
            if (!IsLocalPlayerInTheElevator())
                return;
            
            TeleportLocalPlayer();
        }

        private async void TeleportLocalPlayer()
        {
            ElevatorFloor currentFloor = _elevatorManager.GetCurrentFloor();
            bool isElevatorFound = _levelManager.TryGetElevator(currentFloor, out ElevatorBase elevatorBase);

            if (!isElevatorFound)
                return;

            var playerEntity = PlayerEntity.Instance;
            Transform playerTransform = playerEntity.transform;
            Transform thisElevatorTransform = _elevator.transform;
            Transform targetElevatorTransform = elevatorBase.transform;

            Quaternion playerRotation = playerTransform.rotation;
            Quaternion thisElevatorRotation = thisElevatorTransform.rotation;
            Quaternion targetElevatorRotation = targetElevatorTransform.rotation;

            Vector3 playerLocalPosition = thisElevatorTransform.InverseTransformPoint(playerTransform.position);
            Vector3 rotationDifference = targetElevatorRotation.eulerAngles - thisElevatorRotation.eulerAngles;

            Debug.Log("Start Player Position: " + playerTransform.position);
            Debug.Log("Start Elevator Position: " + thisElevatorTransform.position);
            
            //playerLocalPosition.y += playerEntity.References.Collider.height;
            Debug.Log("Local Player Position: " + playerLocalPosition);
            
            Vector3 position = targetElevatorTransform.position + playerLocalPosition;
            Vector3 eulerAngles = playerRotation.eulerAngles;
            
            eulerAngles.x += rotationDifference.x;
            eulerAngles.y += rotationDifference.y;
            
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            
            Debug.Log("New Player Position: " + position);
            Debug.Log("New Elevator Position: " + targetElevatorTransform.position);
            playerEntity.TeleportPlayer(position, rotation);
            
            await UniTask.DelayFrame(10);
            
            Debug.Log("Player Position after delay: " + playerTransform.position);
        }

        private bool IsLocalPlayerInTheElevator()
        {
            var localPlayer = PlayerEntity.Instance;
            bool isLocalPlayerInTheElevator = false;
            
            foreach (PlayerEntity playerEntity in _playersList)
            {
                bool isMatches = playerEntity == localPlayer;

                if (!isMatches)
                    continue;

                isLocalPlayerInTheElevator = true;
            }

            return isLocalPlayerInTheElevator;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnFloorChanged(ElevatorStaticData data)
        {
            bool isTargetFloor = data.IsTargetFloor;

            if (!isTargetFloor)
                return;
            
            TryTeleportLocalPlayer();
        }
    }
}