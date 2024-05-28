using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ElevatorTeleportTrigger : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator, ILevelProvider levelProvider)
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _levelProvider = levelProvider;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawTrigger;

        [Title(Constants.References)]
        [SerializeField, Required]
        private ElevatorBase _elevator;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawTrigger => _drawTrigger;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<PlayerEntity> _playersList = new();

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private ILevelProvider _levelProvider;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _elevatorsManagerDecorator.OnFloorChangedEvent += OnFloorChanged;

        private void OnDestroy() =>
            _elevatorsManagerDecorator.OnFloorChangedEvent -= OnFloorChanged;

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryTeleportLocalPlayer()
        {
            if (!IsLocalPlayerInTheElevator())
                return;

            TeleportLocalPlayer();
        }

        private void TeleportLocalPlayer()
        {
            Floor currentFloor = _elevatorsManagerDecorator.GetCurrentFloor();
            bool isElevatorFound = _levelProvider.TryGetElevator(currentFloor, out ElevatorBase targetElevator);

            if (!isElevatorFound)
                return;

            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();
            Transform target = playerEntity.transform;
            Transform parent1 = _elevator.transform;
            Transform parent2 = targetElevator.transform;

            GameUtilities.Teleport(target, parent1, parent2, out Vector3 position, out Quaternion rotation);
            playerEntity.TeleportPlayer(position, rotation);
        }

        private bool IsLocalPlayerInTheElevator()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
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