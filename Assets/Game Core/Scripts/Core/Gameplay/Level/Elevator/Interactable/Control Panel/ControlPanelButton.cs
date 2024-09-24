using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Level.Elevator;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public abstract class ControlPanelButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _buttonFloor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnButtonClickedEvent;
        public event Action OnInteractionStateChangedEvent;

        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnButtonEnabled;

        private void OnDestroy() =>
            _animationObserver.OnEnabledEvent -= OnButtonEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
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
            InteractionType.ElevatorFloorButton;

        public bool CanInteract() => _canInteract;


        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleClick()
        {
            SetElevatorFloor();
            OnButtonClickedEvent?.Invoke();
        }
        
        private void SetElevatorFloor()
        {
            ElevatorEntity elevatorEntity = ElevatorEntity.Get();
            elevatorEntity.ChangeTargetFloor(_buttonFloor);
        }

        private void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonEnabled() => ToggleInteract(canInteract: true);
    }
}