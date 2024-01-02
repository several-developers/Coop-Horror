using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersEntity : NetworkBehaviour, IEntity, INetworkObject
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private ToggleMobileDoorLever _toggleMobileDoorLever;

        [SerializeField, Required]
        private LaunchMobileLever _launchMobileLever;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            _animationObserver.OnDoorClosedEvent += OnDoorClosed;
            
            _toggleMobileDoorLever.OnEnabledEvent += OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent += OnDoorLeverDisabled;

            _launchMobileLever.OnEnabledEvent += OnEnableVehicle;
            _launchMobileLever.OnDisabledEvent += OnDisableVehicle;
        }

        public override void OnDestroy()
        {
            _animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
            _animationObserver.OnDoorClosedEvent -= OnDoorClosed;
            
            _toggleMobileDoorLever.OnEnabledEvent -= OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent -= OnDoorLeverDisabled;
            
            _launchMobileLever.OnEnabledEvent -= OnEnableVehicle;
            _launchMobileLever.OnDisabledEvent -= OnDisableVehicle;
            
            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Transform GetTransform() => transform;

        public NetworkObject GetNetworkObject() => _networkObject;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void ChangeVehicleMovementState(bool canMove)
        {
            float motionSpeed = canMove ? 1f : 0f;
            
            _animator.SetBool(id: AnimatorHashes.CanMove, canMove);
            _animator.SetFloat(id: AnimatorHashes.MotionSpeed, motionSpeed);
        }

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnDoorLeverEnabled() => ChangeDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ChangeDoorState(isOpen: false);

        private void OnEnableVehicle()
        {
            ChangeVehicleMovementState(canMove: true);
        }

        private void OnDisableVehicle()
        {
            ChangeVehicleMovementState(canMove: false);
        }
    }
}