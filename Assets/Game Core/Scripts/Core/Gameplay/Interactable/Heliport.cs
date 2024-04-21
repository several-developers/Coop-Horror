using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Interactable
{
    public class Heliport : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsProvider itemsProvider) =>
            _itemsProvider = itemsProvider;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        
        private IItemsProvider _itemsProvider;
        private bool _canInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact(PlayerEntity playerEntity = null)
        {
            if (playerEntity == null)
                return;
            
            TryDropPlayerItem(playerEntity);
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.Heliport;

        public bool CanInteract() =>
            _canInteract;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryDropPlayerItem(PlayerEntity playerEntity)
        {
            PlayerInventory playerInventory = playerEntity.GetInventory();
            bool isItemDataFound = playerInventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

            if (!isItemDataFound)
                return;

            int uniqueItemID = inventoryItemData.UniqueItemID;
            bool isItemFound = _itemsProvider.TryGetItem(uniqueItemID, out ItemObjectBase item);

            if (!isItemFound)
                return;
            
            playerEntity.DropItem();
        }
        
        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();
    }
}