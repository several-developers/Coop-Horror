using System.Collections.Generic;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level.Elevator;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    public class ElevatorTeleportTrigger : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawTrigger;

        [Title(Constants.References)]
        [SerializeField, Required]
        private ElevatorEntity _elevator;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawTrigger => _drawTrigger;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<PlayerEntity> _playersList = new();

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;

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

            Transform elevatorTransform = _elevator.transform;
            _elevatorsManagerDecorator.TeleportLocalPlayer(elevatorTransform);
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