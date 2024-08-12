using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Interactable.MarketShop
{
    public class MarketShopTrigger : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IQuestsManagerDecorator questsManagerDecorator, IItemsProvider itemsProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _itemsProvider = itemsProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private IQuestsManagerDecorator _questsManagerDecorator;
        private IItemsProvider _itemsProvider;
        private bool _canInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            playerInventory.OnSelectedSlotChangedEvent += OnPlayerSelectedSlotChanged;
        }

        public void InteractionEnded(IEntity entity = null)
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            playerInventory.OnSelectedSlotChangedEvent -= OnPlayerSelectedSlotChanged;
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
            InteractionType.MarketShop;

        public bool CanInteract()
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            
            bool isItemInSelectedSlotExists =
                playerInventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

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
        }
        
        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();
        
        private static PlayerInventory GetPlayerInventory()
        {
            PlayerEntity localPlayer = GetLocalPlayer();
            return localPlayer.GetInventory();
        }

        private static PlayerEntity GetLocalPlayer() =>
            PlayerEntity.GetLocalPlayer();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerSelectedSlotChanged(ChangedSlotStaticData data) => SendInteractionStateChangedEvent();
    }
}