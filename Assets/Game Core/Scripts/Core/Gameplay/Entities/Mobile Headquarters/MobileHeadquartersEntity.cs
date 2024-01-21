using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums;
using GameCore.Gameplay.Interactable;
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
        private LaunchMobileLever _launchMobileLever;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<SceneName> OnLoadLocationEvent;
        public event Action OnLocationLeftEvent;
        
        private static MobileHeadquartersEntity _instance;
        
        private RigidbodyPathMovement _pathMovement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            Init();
            
            _animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            _animationObserver.OnDoorClosedEvent += OnDoorClosed;
            
            _toggleMobileDoorLever.OnEnabledEvent += OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent += OnDoorLeverDisabled;

            _launchMobileLever.OnEnabledEvent += OnEnableVehicle;
            _launchMobileLever.OnDisabledEvent += OnDisableVehicle;
        }

        private void FixedUpdate() =>
            _pathMovement.Movement();

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

        public void ChangePath(CinemachinePath path) =>
            _pathMovement.ChangePath(path);

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

        private void ChangeDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnDoorLeverEnabled() => ChangeDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ChangeDoorState(isOpen: false);

        private void OnEnableVehicle()
        {
            OnLoadLocationEvent?.Invoke(SceneName.Desert);
            //ToggleMovement(canMove: true);
        }

        private void OnDisableVehicle()
        {
            OnLocationLeftEvent?.Invoke();
            //ToggleMovement(canMove: false);
        }
    }
}