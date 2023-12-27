using GameCore.Enums;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using GameCore.Observers.Gameplay.PlayerInteraction;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Interaction
{
    public class InteractionHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InteractionHandler(PlayerEntity playerEntity, Transform itemHoldPivot,
            IPlayerInteractionObserver playerInteractionObserver)
        {
            _playerEntity = playerEntity;
            _itemHoldPivot = itemHoldPivot;
            _playerInteractionObserver = playerInteractionObserver;

            _playerInteractionObserver.OnCanInteractEvent += OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly Transform _itemHoldPivot;
        private readonly IPlayerInteractionObserver _playerInteractionObserver;

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

        private void HandlePickUpItem()
        {
            bool isItem = _lastInteractable is IInteractableItem;

            if (!isItem)
            {
                string log = Log.HandleLog("Interactable <rb>is not item</rb>.");
                Debug.Log(log);
                return;
            }
            
            PlayerInventory inventory = _playerEntity.GetInventory();
            bool isInventoryFull = inventory.IsInventoryFull();

            if (isInventoryFull)
            {
                string log = Log.HandleLog("Player inventory <ob>is full</ob>.");
                Debug.Log(log);
                return;
            }

            var interactableItem = (IInteractableItem)_lastInteractable;
            int itemID = interactableItem.GetItemID();
            ItemData itemData = new(itemID);
            
            inventory.AddItem(itemData);
        }

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