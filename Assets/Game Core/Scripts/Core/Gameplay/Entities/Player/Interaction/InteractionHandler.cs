using GameCore.Enums;
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

            _playerInteractionObserver.OnCanInteractEvent += OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerInventoryManager _playerInventoryManager;
        private readonly IPlayerInteractionObserver _playerInteractionObserver;
        private readonly IInteractableItem[] _interactableItems;

        private IInteractable _lastInteractable;
        private bool _canInteract;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _playerInteractionObserver.OnCanInteractEvent -= OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent -= OnInteractionEnded;
        }

        public void Interact()
        {
            if (!_canInteract)
                return;

            InteractionType interactionType = _lastInteractable.GetInteractionType();

            switch (interactionType)
            {
                case InteractionType.PickUpItem:
                    HandlePickUpItem();
                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandlePickUpItem() =>
            _playerInventoryManager.PickUpItem(_lastInteractable);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCanInteract(IInteractable interactable)
        {
            _canInteract = true;
            _lastInteractable = interactable;
        }

        private void OnInteractionEnded() =>
            _canInteract = false;
    }
}