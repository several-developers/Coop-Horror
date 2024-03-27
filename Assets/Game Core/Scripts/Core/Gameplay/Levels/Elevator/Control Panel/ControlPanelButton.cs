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
        private Floor _floor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        public event Action<Floor> OnStartElevatorClickedEvent;

        private ElevatorsManager _elevatorsManager;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnEnabledEvent;

        private void Start()
        {
            _elevatorsManager = ElevatorsManager.Get();
            _elevatorsManager.OnElevatorStoppedEvent += OnElevatorsStopped;
        }

        private void OnDestroy()
        {
            _animationObserver.OnEnabledEvent -= OnEnabledEvent;
            
            _elevatorsManager.OnElevatorStoppedEvent -= OnElevatorsStopped;
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
            OnStartElevatorClickedEvent?.Invoke(_floor);

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
            _elevatorsManager.IsElevatorMoving();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEnabledEvent()
        {
            if (IsElevatorMoving())
                return;
            
            ToggleInteract(canInteract: true);
        }

        private void OnElevatorsStopped(Floor floor) => ToggleInteract(canInteract: true);
    }
}