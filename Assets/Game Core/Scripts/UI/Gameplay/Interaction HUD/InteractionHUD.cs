using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.UI.Global.Animations;
using Sirenix.OdinInspector;
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

        [TitleGroup(Constants.Animation)]
        [BoxGroup(Constants.AnimationIn, showLabel: false), SerializeField]
        private TMPFadeAnimation _fadeAnimation;

        // FIELDS: --------------------------------------------------------------------------------

        private IPlayerInteractionObserver _playerInteractionObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _playerInteractionObserver.OnCanInteractEvent += OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        private void OnDestroy()
        {
            _playerInteractionObserver.OnCanInteractEvent -= OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent -= OnInteractionEnded;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowInteractionInfo(IInteractable interactable)
        {
            _fadeAnimation.Show();
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCanInteract(IInteractable interactable) => ShowInteractionInfo(interactable);

        private void OnInteractionEnded() =>
            _fadeAnimation.Hide();
    }
}