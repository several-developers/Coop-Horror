using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class CallButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private ElevatorBase _elevator;

        [SerializeField, Required]
        private ControlPanel _controlPanel;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private ElevatorManager _elevatorManager;
        private RpcCaller _rpcCaller;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnEnabledEvent;

        private void Start()
        {
            _elevatorManager = ElevatorManager.Get();
            _rpcCaller = RpcCaller.Get();
            
            _elevatorManager.OnElevatorStoppedEvent += OnElevatorStopped;
        }

        private void OnDestroy()
        {
            _animationObserver.OnEnabledEvent -= OnEnabledEvent;
            
            _elevatorManager.OnElevatorStoppedEvent -= OnElevatorStopped;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            if (IsElevatorMoving())
                return;
            
            ToggleInteract(canInteract: false);
            PlayAnimation();
            HandleClick();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent?.Invoke();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.ElevatorCallButton;

        public bool CanInteract() =>
            _canInteract && !IsElevatorMoving();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        private void HandleClick()
        {
            ElevatorFloor elevatorFloor = _elevator.GetElevatorFloor();
            ElevatorFloor currentFloor = _elevatorManager.GetCurrentFloor();
            bool isElevatorMoving = IsElevatorMoving();
            bool openElevator = !isElevatorMoving && currentFloor == elevatorFloor;

            if (openElevator)
                OpenElevator(elevatorFloor);
            else
                StartElevator(elevatorFloor);
        }

        private void OpenElevator(ElevatorFloor elevatorFloor) =>
            _rpcCaller.OpenElevator(elevatorFloor);

        private void StartElevator(ElevatorFloor elevatorFloor) =>
            _controlPanel.StartElevator(elevatorFloor);

        private bool IsElevatorMoving() =>
            _elevatorManager.IsElevatorMoving();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEnabledEvent()
        {
            bool isElevatorMoving = _elevatorManager.IsElevatorMoving();

            if (isElevatorMoving)
                return;
            
            ToggleInteract(canInteract: true);
        }
        
        private void OnElevatorStopped(ElevatorFloor elevatorFloor) => ToggleInteract(canInteract: true);
    }
}