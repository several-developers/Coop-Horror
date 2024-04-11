using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Interactable
{
    public class SimpleButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        public event Action OnTriggerEvent = delegate { };
        
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnInteractEvent += OnButtonTriggered;

        private void OnDestroy() =>
            _animationObserver.OnInteractEvent -= OnButtonTriggered;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            _canInteract = false;
            _animator.SetTrigger(id: AnimatorHashes.Trigger);
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.SimpleButton;

        public bool CanInteract() => _canInteract;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonTriggered()
        {
            _canInteract = true;
            OnTriggerEvent.Invoke();
        }
    }
}