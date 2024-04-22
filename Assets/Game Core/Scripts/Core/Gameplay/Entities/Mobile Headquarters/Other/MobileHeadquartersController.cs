using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Gameplay.Other;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersController : INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MobileHeadquartersController(MobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _references = mobileHeadquartersEntity.References;
            _rpcHandlerDecorator = mobileHeadquartersEntity.RpcHandlerDecorator;
            _gameManagerDecorator = mobileHeadquartersEntity.GameManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly MobileHeadquartersReferences _references;
        private readonly IRpcHandlerDecorator _rpcHandlerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void InitServerAndClient()
        {
            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            animationObserver.OnDoorClosedEvent += OnDoorClosed;

            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.OnInteractEvent += OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent += OnLoadLocation;

            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.OnInteractEvent += OnInteractWithLeaveLocationLever;
            leaveLocationLever.OnEnabledEvent += OnStartLeavingLocation;
        }
        
        public void InitServer()
        {
        }

        public void InitClient()
        {
        }
        
        public void DespawnServerAndClient()
        {
            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
            animationObserver.OnDoorClosedEvent -= OnDoorClosed;

            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.OnInteractEvent -= OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent -= OnLoadLocation;

            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.OnInteractEvent -= OnInteractWithLeaveLocationLever;
            leaveLocationLever.OnEnabledEvent -= OnStartLeavingLocation;
        }
        
        public void DespawnServer()
        {
        }

        public void DespawnClient()
        {
            
        }

        public void ToggleDoorState(bool isOpen)
        {
            Animator animator = _references.Animator;
            animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDoorOpened()
        {
        }

        private void OnDoorClosed()
        {
        }

        private void OnInteractWithLoadLocationLever() =>
            _mobileHeadquartersEntity.LoadLocationServerRpc();

        private void OnLoadLocation()
        {
            SceneName locationName = _gameManagerDecorator.GetSelectedLocation();
            _rpcHandlerDecorator.LoadLocation(locationName);
        }

        private void OnInteractWithLeaveLocationLever()
        {
            LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
            leaveLocationLever.InteractLogic();
        }

        private void OnStartLeavingLocation() =>
            _rpcHandlerDecorator.StartLeavingLocation();
    }
}