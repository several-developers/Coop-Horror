using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network.Other;
using GameCore.Utilities;
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
            
            TeleportLocalPlayer2();
        }

        private void TeleportLocalPlayer2()
        {
            Floor currentFloor = _elevatorManager.GetCurrentFloor();
            bool isElevatorFound = _levelManager.TryGetElevator(currentFloor, out ElevatorBase targetElevator);

            if (!isElevatorFound)
                return;
            
            var playerEntity = PlayerEntity.Instance;
            Transform target = playerEntity.transform;
            Transform parent1 = _elevator.transform;
            Transform parent2 = targetElevator.transform;
            
            GameUtilities.Teleport(target, parent1, parent2, out Vector3 position, out Quaternion rotation);
            playerEntity.TeleportPlayer(position, rotation);
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