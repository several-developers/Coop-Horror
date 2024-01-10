using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Inventory
{
    public class PlayerInventoryManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerInventoryManager(PlayerEntity playerEntity, IItemsPreviewFactory itemsPreviewFactory)
        {
            _playerEntity = playerEntity;
            _itemsPreviewFactory = itemsPreviewFactory;
            _playerInventory = playerEntity.GetInventory();
            _itemHoldPivot = playerEntity.GetItemHoldPivot();
            _interactableItems = new IInteractableItem[Constants.PlayerInventorySize];
            _itemsPreviewObjects = new ItemPreviewObject[Constants.PlayerInventorySize];

            _playerInventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            _playerInventory.OnItemDroppedEvent += OnItemDropped;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerInventory _playerInventory;
        private readonly Transform _itemHoldPivot;
        private readonly IItemsPreviewFactory _itemsPreviewFactory;
        private readonly IInteractableItem[] _interactableItems;
        private readonly ItemPreviewObject[] _itemsPreviewObjects;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _playerInventory.OnItemDroppedEvent -= OnItemDropped;

        public void PickUpItem(IInteractable interactable)
        {
            bool isItem = interactable.IsItem();

            if (!isItem)
                return;

            PlayerInventory inventory = _playerEntity.GetInventory();

            if (IsInventoryFull())
                return;

            var interactableItem = (IInteractableItem)interactable;

            int itemID = interactableItem.GetItemID();
            ItemData itemData = new(itemID);
            bool isItemAdded = inventory.AddItem(itemData, out int slotIndex);

            // Not added.
            if (!isItemAdded)
                return;

            NetworkObject playerNetworkObject = _playerEntity.GetNetworkObject();
            interactableItem.PickUp(playerNetworkObject);

            _interactableItems[slotIndex] = interactableItem;

            CreateItemPreview(slotIndex, itemID);
            ToggleItemsState();

            // LOCAL METHODS: -----------------------------

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateItemPreview(int slotIndex, int itemID)
        {
            bool isItemFound = _itemsPreviewFactory.Create(itemID, _itemHoldPivot, out ItemPreviewObject itemPreview);

            if (!isItemFound)
                return;

            _itemsPreviewObjects[slotIndex] = itemPreview;
        }

        private void ToggleItemsState()
        {
            int selectedSlotIndex = _playerInventory.GetSelectedSlotIndex();
            int iterations = _playerInventory.Size;

            for (int i = 0; i < iterations; i++)
            {
                bool show = i == selectedSlotIndex;
                IInteractableItem interactableItem = _interactableItems[i];
                bool isInteractableItemExists = interactableItem != null;

                if (isInteractableItemExists)
                {
                    interactableItem.Hide();

                    if (show)
                    {
                        //interactableItem.Show();
                        interactableItem.ShowServer();
                    }
                    else
                    {
                        interactableItem.HideServer();
                    }
                }

                ItemPreviewObject itemPreviewObject = _itemsPreviewObjects[i];

                if (itemPreviewObject == null)
                    continue;

                if (show)
                {
                    if (isInteractableItemExists)
                        interactableItem.ChangeFollowTarget(itemPreviewObject.transform);

                    itemPreviewObject.Show();
                }
                else
                    itemPreviewObject.Hide();
            }
        }

        private void DropItem(int slotIndex, bool randomPosition)
        {
            IInteractableItem interactableItem = _interactableItems[slotIndex];
            ItemPreviewObject itemPreviewObject = _itemsPreviewObjects[slotIndex];
            
            interactableItem?.DropServer(randomPosition); // Server RPC
            interactableItem?.Drop(randomPosition);

            if (itemPreviewObject != null)
                itemPreviewObject.Drop();

            _interactableItems[slotIndex] = null;
            _itemsPreviewObjects[slotIndex] = null;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedSlotChanged(int selectedSlotIndex) => ToggleItemsState();

        private void OnItemDropped(int slotIndex, bool randomPosition) => DropItem(slotIndex, randomPosition);
    }
}