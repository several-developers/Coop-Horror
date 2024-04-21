using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
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
        private bool _isLeverPulledFromStart;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractEvent; 
        public event Action OnInteractionStateChangedEvent;
        public event Action OnEnabledEvent;
        public event Action OnDisabledEvent;

        private bool _ignoreAnimationEvents;
        private bool _canInteract = true;
        private bool _isLeverPulled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _canInteract = _canInteractFromStart;
            _isLeverPulled = _isLeverPulledFromStart;
            
            if (_isLeverPulledFromStart)
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
        
        public void Interact(PlayerEntity playerEntity = null)
        {
            _canInteract = false;
            OnInteractEvent?.Invoke();
        }

        public void InteractLogic()
        {
            _ignoreAnimationEvents = false;
            _canInteract = false;
            _isLeverPulled = !_isLeverPulled;

            SendInteractionStateChangedEvent();
            ChangeAnimationState();
        }
        
        public void InteractWithoutEvents(bool isLeverPulled)
        {
            _ignoreAnimationEvents = true;
            _isLeverPulled = isLeverPulled;
            
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
            _animator.SetBool(id: AnimatorHashes.IsOn, _isLeverPulled);

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