using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public abstract class ControlPanelButton : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

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

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent += OnElevatorsStopped;
            
            _animationObserver.OnEnabledEvent += OnEnabledEvent;
        }

        private void OnDestroy()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent -= OnElevatorsStopped;
            
            _animationObserver.OnEnabledEvent -= OnEnabledEvent;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted()
        {
        }

        public void InteractionEnded()
        {
        }

        public void Interact(PlayerEntity playerEntity = null)
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
            _elevatorsManagerDecorator.IsElevatorMoving();
        
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