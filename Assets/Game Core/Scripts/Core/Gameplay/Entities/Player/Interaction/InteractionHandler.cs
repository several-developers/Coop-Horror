using GameCore.Enums;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using GameCore.Observers.Gameplay.PlayerInteraction;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Interaction
{
    public class InteractionHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InteractionHandler(PlayerEntity playerEntity, IPlayerInteractionObserver playerInteractionObserver)
        {
            _playerEntity = playerEntity;
            _playerInventory = playerEntity.GetInventory();
            _playerInteractionObserver = playerInteractionObserver;
            _interactableItems = new IInteractableItem[Constants.PlayerInventorySize];

            _playerInventory.OnItemDroppedEvent += OnItemDropped;
            
            _playerInteractionObserver.OnCanInteractEvent += OnCanInteract;
            _playerInteractionObserver.OnInteractionEndedEvent += OnInteractionEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerInventory _playerInventory;
        private readonly IPlayerInteractionObserver _playerInteractionObserver;
        private readonly IInteractableItem[] _interactableItems;

        private IInteractable _lastInteractable;
        private bool _canInteract;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _playerInventory.OnItemDroppedEvent -= OnItemDropped;
            
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
            if (!IsItem())
                return;

            PlayerInventory inventory = _playerEntity.GetInventory();

            if (IsInventoryFull())
                return;

            var interactableItem = (IInteractableItem)_lastInteractable;
            
            int itemID = interactableItem.GetItemID();
            ItemData itemData = new(itemID);
            bool isItemAdded = inventory.AddItem(itemData, out int slotIndex);

            // Not added.
            if (!isItemAdded)
                return;
            
            NetworkObject playerNetworkObject = _playerEntity.GetNetworkObject();
            interactableItem.PickUp(playerNetworkObject);

            _interactableItems[slotIndex] = interactableItem;

            //NetworkObject itemNetworkObject = interactableItem.GetNetworkObject();
            //_networkSpawner.DestroyObject(itemNetworkObject);

            // LOCAL METHODS: -----------------------------

            bool IsItem()
            {
                bool isItem = _lastInteractable is IInteractableItem;

                if (isItem)
                    return true;

                string log = Log.HandleLog("Interactable <rb>is not item</rb>.");
                Debug.Log(log);

                return false;
            }

            bool IsInventoryFull()
            {
                bool isInventoryFull = inventory.IsInventoryFull();

                if (!isInventoryFull)
                    return false;

                string log = Log.HandleLog("Player inventory <ob>is full</ob>.");
                Debug.Log(log);

                return true;
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCanInteract(IInteractable interactable)
        {
            _canInteract = true;
            _lastInteractable = interactable;
        }

        private void OnInteractionEnded() =>
            _canInteract = false;

        private void OnItemDropped(int slotIndex)
        {
            IInteractableItem interactableItem = _interactableItems[slotIndex];
            interactableItem?.Drop();

            _interactableItems[slotIndex] = null;
        }
    }
}