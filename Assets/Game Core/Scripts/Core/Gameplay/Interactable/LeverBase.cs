﻿using System;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Utilities;
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
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractEvent; 
        public event Action OnInteractionStateChangedEvent;
        public event Action OnEnabledEvent;
        public event Action OnDisabledEvent;

        private const string LeverOnAnimation = "Lever On";
        private const string LeverOffAnimation = "Lever Off";

        private float _leverEnablingDuration;
        private float _leverDisablingDuration;
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

            SetupAnimationsDuration();
            ChangeAnimationState();
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
            ChangeState(_isLeverPulled);
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

        private void SetupAnimationsDuration()
        {
            AnimationClip[] animationClips = _animator.runtimeAnimatorController.animationClips;

            foreach (AnimationClip animationClip in animationClips)
            {
                bool isOnAnimation = string.Equals(a: animationClip.name, b: LeverOnAnimation);
                bool isOffAnimation = string.Equals(a: animationClip.name, b: LeverOffAnimation);

                if (isOnAnimation)
                    _leverEnablingDuration = animationClip.length;

                if (isOffAnimation)
                    _leverDisablingDuration = animationClip.length;
            }
        }

        private async void ChangeState(bool isPulled)
        {
            float delaySeconds = isPulled ? _leverEnablingDuration : _leverDisablingDuration;
            int delay = delaySeconds.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            if (IsIgnoringAnimationEvents())
                return;
            
            if (isPulled)
                SendEnabledEvent();
            else
                SendDisabledEvent();
        }

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
    }
}