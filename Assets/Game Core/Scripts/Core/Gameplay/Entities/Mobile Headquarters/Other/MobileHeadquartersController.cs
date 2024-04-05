using GameCore.Enums.Global;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MobileHeadquartersController(MobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _references = mobileHeadquartersEntity.References;
            _rpcHandlerDecorator = mobileHeadquartersEntity.RpcHandlerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly MobileHeadquartersReferences _references;
        private readonly IRpcHandlerDecorator _rpcHandlerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init()
        {
            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            animationObserver.OnDoorClosedEvent += OnDoorClosed;

            ToggleMobileDoorLever toggleMobileDoorLever = _references.ToggleMobileDoorLever;
            toggleMobileDoorLever.OnInteractEvent += OnInteractWithMobileDoorLever;
            toggleMobileDoorLever.OnEnabledEvent += OnDoorLeverEnabled;
            toggleMobileDoorLever.OnDisabledEvent += OnDoorLeverDisabled;

            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.OnInteractEvent += OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent += OnLoadLocation;

            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.OnInteractEvent += OnInteractWithLeaveLocationLever;
            leaveLocationLever.OnEnabledEvent += OnLeaveLocation;
        }

        public void Dispose()
        {
            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
            animationObserver.OnDoorClosedEvent -= OnDoorClosed;

            ToggleMobileDoorLever toggleMobileDoorLever = _references.ToggleMobileDoorLever;
            toggleMobileDoorLever.OnInteractEvent -= OnInteractWithMobileDoorLever;
            toggleMobileDoorLever.OnEnabledEvent -= OnDoorLeverEnabled;
            toggleMobileDoorLever.OnDisabledEvent -= OnDoorLeverDisabled;

            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.OnInteractEvent -= OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent -= OnLoadLocation;

            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.OnInteractEvent -= OnInteractWithLeaveLocationLever;
            leaveLocationLever.OnEnabledEvent -= OnLeaveLocation;
        }

        public void ToggleDoorState(bool isOpen)
        {
            Animator animator = _references.Animator;
            animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableDoorLever()
        {
            ToggleMobileDoorLever toggleMobileDoorLever = _references.ToggleMobileDoorLever;
            toggleMobileDoorLever.ToggleInteract(canInteract: true);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnInteractWithMobileDoorLever() =>
            _mobileHeadquartersEntity.InteractWithMobileDoorLeverServerRpc();

        private void OnDoorLeverEnabled() => ToggleDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ToggleDoorState(isOpen: false);

        private void OnInteractWithLoadLocationLever() =>
            _mobileHeadquartersEntity.InteractWithLoadLocationLeverServerRpc();

        private void OnLoadLocation() =>
            _rpcHandlerDecorator.LoadLocation(SceneName.Desert);

        private void OnInteractWithLeaveLocationLever()
        {
            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.InteractLogic();
        }

        private void OnLeaveLocation() =>
            _rpcHandlerDecorator.StartLeavingLocation();
    }
}