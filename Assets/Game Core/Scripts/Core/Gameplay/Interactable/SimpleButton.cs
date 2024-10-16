﻿using System;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Interactable
{
    public class SimpleButton : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _delay = 0.15f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        public event Action OnTriggerEvent = delegate { };
        
        protected bool IsInteractionEnabled = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void InteractionStarted(IEntity entity = null)
        {
        }

        public virtual void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
            IsInteractionEnabled = false;
            InteractLogic();
            PlayInteractAnimation();
        }

        public void PlayInteractAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        public void ToggleInteract(bool canInteract)
        {
            IsInteractionEnabled = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.SimpleButton;

        public virtual bool CanInteract() => IsInteractionEnabled;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void InteractLogic()
        {
            int delay = _delay.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            IsInteractionEnabled = true;
            OnTriggerEvent.Invoke();
        }
    }
}