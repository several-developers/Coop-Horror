using System;
using GameCore.Enums;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public abstract class ControlPanelButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;

        protected RpcCaller RPCCaller;
        
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnEnabledEvent;

        private void Start() =>
            RPCCaller = RpcCaller.Get();

        private void OnDestroy() =>
            _animationObserver.OnEnabledEvent -= OnEnabledEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract void Interact();

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.ElevatorFloorButton;

        public bool CanInteract() => _canInteract;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEnabledEvent() => ToggleInteract(canInteract: true);
    }
}