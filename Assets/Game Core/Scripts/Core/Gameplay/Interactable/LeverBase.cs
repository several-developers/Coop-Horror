using System;
using GameCore.Enums;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Interactable
{
    public abstract class LeverBase : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _canInteractFromStart;
        
        [SerializeField]
        private bool _isEnabledFromStart;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;
        public event Action OnEnabledEvent;
        public event Action OnDisabledEvent;

        private bool _ignoreAnimationEvents;
        private bool _canInteract = true;
        private bool _isEnabled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _canInteract = _canInteractFromStart;
            _isEnabled = _isEnabledFromStart;
            
            if (_isEnabledFromStart)
                _ignoreAnimationEvents = true;

            ChangeAnimationState();

            _animationObserver.OnEnabledEvent += OnLeverEnabled;
            _animationObserver.OnDisabledEvent += OnLeverDisabled;
        }

        private void OnDestroy()
        {
            _animationObserver.OnEnabledEvent -= OnLeverEnabled;
            _animationObserver.OnDisabledEvent -= OnLeverDisabled;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            _isEnabled = !_isEnabled;
            _canInteract = false;

            SendInteractionStateChangedEvent();
            ChangeAnimationState();
        }

        public void InteractWithoutEvents(bool isEnabled)
        {
            _ignoreAnimationEvents = true;
            _isEnabled = isEnabled;
            
            ChangeAnimationState();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        public abstract InteractionType GetInteractionType();

        public bool CanInteract() => _canInteract;
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeAnimationState() =>
            _animator.SetBool(id: AnimatorHashes.IsOn, _isEnabled);

        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        private void SendEnabledEvent() =>
            OnEnabledEvent?.Invoke();

        private void SendDisabledEvent() =>
            OnDisabledEvent?.Invoke();

        private bool IsIgnoringAnimationEvents()
        {
            if (!_ignoreAnimationEvents)
                return false;
            
            _ignoreAnimationEvents = false;
            return true;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected virtual void OnLeverEnabled()
        {
            if (IsIgnoringAnimationEvents())
                return;
            
            SendEnabledEvent();
        }

        protected virtual void OnLeverDisabled()
        {
            if (IsIgnoringAnimationEvents())
                return;
            
            SendDisabledEvent();
        }
    }
}