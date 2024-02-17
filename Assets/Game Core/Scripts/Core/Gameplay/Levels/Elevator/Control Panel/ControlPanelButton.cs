using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public abstract class ControlPanelButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private ElevatorFloor _elevatorFloor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        public event Action<ElevatorFloor> OnStartElevatorClickedEvent;

        private ElevatorManager _elevatorManager;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnEnabledEvent;

        private void Start()
        {
            _elevatorManager = ElevatorManager.Get();
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
            ToggleInteract(canInteract: false);
            PlayAnimation();
            StartElevator();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        private void StartElevator() =>
            OnStartElevatorClickedEvent?.Invoke(_elevatorFloor);

        public InteractionType GetInteractionType() =>
            InteractionType.ElevatorFloorButton;

        public bool CanInteract() =>
            _canInteract && !IsElevatorMoving();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        private bool IsElevatorMoving() =>
            _elevatorManager.IsElevatorMoving();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEnabledEvent()
        {
            if (IsElevatorMoving())
                return;
            
            ToggleInteract(canInteract: true);
        }

        private void OnElevatorStopped(ElevatorFloor elevatorFloor) => ToggleInteract(canInteract: true);
    }
}