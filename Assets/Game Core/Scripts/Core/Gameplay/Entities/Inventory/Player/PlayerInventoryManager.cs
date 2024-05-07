using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using GameCore.Utilities;
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
            _cameraItemPivot = playerEntity.GetCameraItemPivot();
            _playerItemPivot = playerEntity.GetRightHandItemsHolder();
            _interactableItems = new IInteractableItem[Constants.PlayerInventorySize];
            _itemsPreviewObjects = new ItemPreviewObject[Constants.PlayerInventorySize];

            _playerInventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            _playerInventory.OnItemDroppedEvent += OnItemDropped;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerInventory _playerInventory;
        private readonly Transform _cameraItemPivot;
        private readonly Transform _playerItemPivot;
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

            int itemID = interactableItem.ItemID;
            int uniqueItemID = interactableItem.UniqueItemID;
            InventoryItemData inventoryItemData = new(itemID, uniqueItemID);
            bool isItemAdded = inventory.AddItem(inventoryItemData, out int slotIndex);

            // Not added.
            if (!isItemAdded)
                return;
            
            interactableItem.PickUp();

            _interactableItems[slotIndex] = interactableItem;

            CreateItemPreviewClientSide(slotIndex, itemID, isFirstPerson: true);
            CreateItemPreviewServerSide(slotIndex, itemID);
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

        public void CreateItemPreviewClientSide(int slotIndex, int itemID, bool isFirstPerson)
        {
            bool isItemPreviewExists = _itemsPreviewObjects[slotIndex] != null;

            // Prevent creation of the second preview item for the item owner.
            if (isItemPreviewExists)
                return;

            Transform itemPivot = isFirstPerson ? _cameraItemPivot : _playerItemPivot;

            bool isItemCreated = _itemsPreviewFactory.Create(itemID, itemPivot, isFirstPerson,
                out ItemPreviewObject itemPreview);

            if (!isItemCreated)
                return;

            bool isSelected = _playerInventory.GetSelectedSlotIndex() == slotIndex;

            if (!isSelected)
                itemPreview.Hide();

            _itemsPreviewObjects[slotIndex] = itemPreview;
        }

        public void DestroyItemPreview(int slotIndex)
        {
            ItemPreviewObject itemPreviewObject = _itemsPreviewObjects[slotIndex];

            if (itemPreviewObject != null)
                itemPreviewObject.Drop();

            _itemsPreviewObjects[slotIndex] = null;
        }

        public void ToggleItemsState()
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
                    //interactableItem.HideClient();

                    if (show)
                    {
                        //interactableItem.ShowClient();
                        //interactableItem.ShowServer();
                    }
                    else
                    {
                        //interactableItem.HideServer();
                    }
                }

                ItemPreviewObject itemPreviewObject = _itemsPreviewObjects[i];

                if (itemPreviewObject == null)
                    continue;

                if (show)
                    itemPreviewObject.Show();
                else
                    itemPreviewObject.Hide();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateItemPreviewServerSide(int slotIndex, int itemID) =>
            _playerEntity.CreateItemPreviewServerRpc(slotIndex, itemID);

        private void DropItem(DroppedItemStaticData data)
        {
            int slotIndex = data.SlotIndex;
            bool randomPosition = data.RandomPosition;
            bool destroy = data.Destroy;
            
            IInteractableItem interactableItem = _interactableItems[slotIndex];
            Vector3 position;
            Quaternion rotation;

            if (randomPosition)
            {
                position = _playerEntity.transform.position;
                rotation = Quaternion.identity;
            }
            else
            {
                ItemPreviewObject itemPreviewObject = _itemsPreviewObjects[slotIndex];
                Transform itemPreviewTransform = itemPreviewObject.transform;
                position = itemPreviewTransform.position;
                rotation = itemPreviewTransform.rotation;
            }

            interactableItem?.Drop(position, rotation, randomPosition, destroy);

            _interactableItems[slotIndex] = null;

            _playerEntity.DestroyItemPreviewServerRpc(slotIndex);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedSlotChanged(ChangedSlotStaticData data) => ToggleItemsState();

        private void OnItemDropped(DroppedItemStaticData data) => DropItem(data);
    }
}