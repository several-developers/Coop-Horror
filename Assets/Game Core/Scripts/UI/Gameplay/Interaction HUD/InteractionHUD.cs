﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.UI.Global.Animations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.Interaction
{
    public class InteractionHUD : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IPlayerInteractionObserver playerInteractionObserver) =>
            _playerInteractionObserver = playerInteractionObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private InteractionTextSettings _interactionTextSettings;

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        [TitleGroup(Constants.Animation)]
        [BoxGroup(Constants.AnimationIn, showLabel: false), SerializeField]
        private TMPFadeAnimation _fadeAnimation;

        // FIELDS: --------------------------------------------------------------------------------

        private IPlayerInteractionObserver _playerInteractionObserver;
        private IInteractable _lastInteractable;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _interactionTextSettings.Setup(_textTMP);

            _playerInteractionObserver.OnInteractionStartedEvent += OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        private void OnDestroy()
        {
            _playerInteractionObserver.OnInteractionStartedEvent -= OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent -= OnInteractionEnded;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowInteractionInfo(IInteractable interactable)
        {
            InteractionType interactionType = interactable.GetInteractionType();
            bool canInteract = interactable.CanInteract();

            _interactionTextSettings.UpdateText(interactionType, canInteract);
        }

        private void SubscribeLastInteractable(IInteractable interactable)
        {
            _lastInteractable = interactable;
            _lastInteractable.OnInteractionStateChangedEvent += OnInteractionStateChanged;
        }

        private void UnsubscribeLastInteractable()
        {
            if (_lastInteractable == null)
                return;

            _lastInteractable.OnInteractionStateChangedEvent -= OnInteractionStateChanged;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCanInteract(IInteractable interactable)
        {
            UnsubscribeLastInteractable();
            SubscribeLastInteractable(interactable);
            ShowInteractionInfo(interactable);
            _fadeAnimation.Show();
        }

        private void OnInteractionEnded()
        {
            UnsubscribeLastInteractable();
            _fadeAnimation.Hide();
        }

        private void OnInteractionStateChanged() => ShowInteractionInfo(_lastInteractable);
    }
}