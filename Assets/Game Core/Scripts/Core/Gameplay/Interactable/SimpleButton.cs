using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
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
        
        protected bool IsInteractionEnabled = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnInteractEvent += OnButtonTriggered;

        private void OnDestroy() =>
            _animationObserver.OnInteractEvent -= OnButtonTriggered;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact(PlayerEntity playerEntity = null)
        {
            IsInteractionEnabled = false;
            _animator.SetTrigger(id: AnimatorHashes.Trigger);
        }

        public void ToggleInteract(bool canInteract)
        {
            IsInteractionEnabled = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.SimpleButton;

        public virtual bool CanInteract() => IsInteractionEnabled;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonTriggered()
        {
            IsInteractionEnabled = true;
            OnTriggerEvent.Invoke();
        }
    }
}