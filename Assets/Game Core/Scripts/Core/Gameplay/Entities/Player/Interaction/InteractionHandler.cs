using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using GameCore.Observers.Gameplay.PlayerInteraction;

namespace GameCore.Gameplay.Entities.Player.Interaction
{
    public class InteractionHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InteractionHandler(PlayerInventoryManager playerInventoryManager,
            IPlayerInteractionObserver playerInteractionObserver)
        {
            _playerInventoryManager = playerInventoryManager;
            _playerInteractionObserver = playerInteractionObserver;
            _interactableItems = new IInteractableItem[Constants.PlayerInventorySize];

            _playerInteractionObserver.OnInteractionStartedEvent += OnInteractionStarted;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerInventoryManager _playerInventoryManager;
        private readonly IPlayerInteractionObserver _playerInteractionObserver;
        private readonly IInteractableItem[] _interactableItems;

        private IInteractable _lastInteractable;
        private bool _isInteractableFound;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _playerInteractionObserver.OnInteractionStartedEvent -= OnInteractionStarted;
            _playerInteractionObserver.OnInteractionEndedEvent -= OnInteractionEnded;
        }

        public void Interact()
        {
            if (!_isInteractableFound)
                return;

            InteractionType interactionType = _lastInteractable.GetInteractionType();

            if (!_lastInteractable.CanInteract())
                return;

            switch (interactionType)
            {
                case InteractionType.PickUpItem:
                    HandlePickUpItem();
                    break;

                default:
                    _lastInteractable.Interact();
                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandlePickUpItem() =>
            _playerInventoryManager.PickUpItem(_lastInteractable);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnInteractionStarted(IInteractable interactable)
        {
            _isInteractableFound = true;
            _lastInteractable = interactable;
        }

        private void OnInteractionEnded() =>
            _isInteractableFound = false;
    }
}