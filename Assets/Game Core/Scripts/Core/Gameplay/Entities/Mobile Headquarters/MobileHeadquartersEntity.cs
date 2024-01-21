using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersEntity : NetworkBehaviour, IMobileHeadquartersEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private MobileHeadquartersConfigMeta _mobileHeadquartersConfig;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        [SerializeField, Required]
        private CinemachineDollyCart _dollyCart;
        
        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private ToggleMobileDoorLever _toggleMobileDoorLever;
        
        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<SceneName> OnLoadLocationEvent;
        public event Action OnLocationLeftEvent;
        
        private static MobileHeadquartersEntity _instance;
        
        private RigidbodyPathMovement _pathMovement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            Init();

            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;
            
            _animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            _animationObserver.OnDoorClosedEvent += OnDoorClosed;
            
            _toggleMobileDoorLever.OnEnabledEvent += OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent += OnDoorLeverDisabled;
            
            _loadLocationLever.OnEnabledEvent += OnLoadLocation;

            _leaveLocationLever.OnEnabledEvent += OnLeaveLocation;
        }

        private void FixedUpdate()
        {
            if (IsOwner)
                _pathMovement.Movement();
        }

        public override void OnDestroy()
        {
            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;
            
            _animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
            _animationObserver.OnDoorClosedEvent -= OnDoorClosed;
            
            _toggleMobileDoorLever.OnEnabledEvent -= OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent -= OnDoorLeverDisabled;
            
            _loadLocationLever.OnEnabledEvent -= OnLoadLocation;
            
            _leaveLocationLever.OnEnabledEvent -= OnLeaveLocation;
            
            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ChangePath(CinemachinePath path) =>
            _pathMovement.ChangePath(path);

        public void ArrivedAtLocation()
        {
            OnDestinationReached();
        }

        public void LeftLocation()
        {
            _loadLocationLever.InteractWithoutEvents(isEnabled: false);
            _loadLocationLever.ToggleInteract(canInteract: true);
        }

        public Transform GetTransform() => transform;

        public NetworkObject GetNetworkObject() => _networkObject;

        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward * _dollyCart.m_Speed;
            return direction;
        }

        public static MobileHeadquartersEntity Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            _instance = this;
            _pathMovement = new RigidbodyPathMovement(transform, _animator, _dollyCart, _mobileHeadquartersConfig);
        }

        [Button]
        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);
        
        private void ToggleDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        [Button]
        private void OnDestinationReached()
        {
            _leaveLocationLever.InteractWithoutEvents(isEnabled: false);
            _leaveLocationLever.ToggleInteract(canInteract: true);
        }
        
        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnDoorLeverEnabled() => ToggleDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ToggleDoorState(isOpen: false);
        
        private void OnLoadLocation() =>
            OnLoadLocationEvent?.Invoke(SceneName.Desert);

        private void OnLeaveLocation() =>
            OnLocationLeftEvent?.Invoke();
    }
}