using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.PlayerInteraction;

namespace GameCore.Gameplay.Entities.Player.Interaction
{
    public class InteractionHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InteractionHandler(PlayerEntity playerEntity, PlayerInventoryManager playerInventoryManager,
            IPlayerInteractionObserver playerInteractionObserver)
        {
            _playerEntity = playerEntity;
            _playerInventoryManager = playerInventoryManager;
            _playerInteractionObserver = playerInteractionObserver;

            _playerInteractionObserver.OnInteractionStartedEvent += OnInteractionStarted;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerInventoryManager _playerInventoryManager;
        private readonly IPlayerInteractionObserver _playerInteractionObserver;

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
                    _lastInteractable.Interact(_playerEntity);
                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandlePickUpItem() =>
            _playerInventoryManager.PickUpItem(_lastInteractable);

        private void InteractionStarted() =>
            _lastInteractable.InteractionStarted(_playerEntity);

        private void InteractionEnded() =>
            _lastInteractable.InteractionEnded(_playerEntity);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnInteractionStarted(IInteractable interactable)
        {
            if (_isInteractableFound)
                InteractionEnded();

            _isInteractableFound = true;
            _lastInteractable = interactable;
            
            InteractionStarted();
        }

        private void OnInteractionEnded()
        {
            _isInteractableFound = false;
            
            InteractionEnded();
        }
    }
}