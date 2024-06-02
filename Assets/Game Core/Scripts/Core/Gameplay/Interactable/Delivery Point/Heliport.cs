using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.EntitiesSystems.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Interactable
{
    public class Heliport : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsProvider itemsProvider, IQuestsManagerDecorator questsManagerDecorator,
            IDeliveryManagerDecorator deliveryManagerDecorator)
        {
            _itemsProvider = itemsProvider;
            _questsManagerDecorator = questsManagerDecorator;
            _deliveryManagerDecorator = deliveryManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;
        
        private IItemsProvider _itemsProvider;
        private IQuestsManagerDecorator _questsManagerDecorator;
        private IDeliveryManagerDecorator _deliveryManagerDecorator;
        private bool _canInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            PlayerInventory inventory = localPlayer.GetInventory();
            inventory.OnSelectedSlotChangedEvent += OnPlayerSelectedSlotChanged;
        }

        public void InteractionEnded()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            PlayerInventory inventory = localPlayer.GetInventory();
            inventory.OnSelectedSlotChangedEvent -= OnPlayerSelectedSlotChanged;
        }

        public void Interact(IEntity entity = null)
        {
            if (entity == null)
                return;

            bool isPlayer = entity.GetType() == typeof(PlayerEntity);

            if (!isPlayer)
                return;
            
            var playerEntity = entity as PlayerEntity;
            
            TryDropPlayerItem(playerEntity);
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            SendInteractionStateChangedEvent();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.Heliport;

        public bool CanInteract()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            PlayerInventory inventory = localPlayer.GetInventory();
            bool isItemInSelectedSlotExists = inventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

            if (!isItemInSelectedSlotExists)
                return false;

            int itemID = inventoryItemData.ItemID;
            bool containsItemInQuests = _questsManagerDecorator.ContainsItemInQuests(itemID);

            if (!containsItemInQuests)
                return false;
            
            return _canInteract;
        }

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
            
            playerEntity.DropItem(destroy: true);
            _questsManagerDecorator.SubmitQuestItem(item.ItemID);
            _deliveryManagerDecorator.ResetTakeOffTimer();
        }
        
        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerSelectedSlotChanged(ChangedSlotStaticData data) => SendInteractionStateChangedEvent();
    }
}