using GameCore.Gameplay.Entities.Player;
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

        public PlayerInventoryManager(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _playerInventory = playerEntity.GetInventory();
            _interactableItems = new IInteractableItem[Constants.PlayerInventorySize];

            _playerInventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            _playerInventory.OnItemDroppedEvent += OnItemDropped;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerInventory _playerInventory;
        private readonly IInteractableItem[] _interactableItems;

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

        private void ToggleItemsState()
        {
            int selectedSlotIndex = _playerInventory.GetSelectedSlotIndex();
            int iterations = _playerInventory.Size;

            for (int i = 0; i < iterations; i++)
            {
                bool show = i == selectedSlotIndex;
                IInteractableItem interactableItem = _interactableItems[i];

                if (interactableItem == null)
                    continue;

                if (show)
                {
                    interactableItem.Show();
                    interactableItem.ShowServer();
                }
                else
                {
                    interactableItem.Hide();
                    interactableItem.HideServer();
                }
            }
        }

        private void DropItem(int slotIndex, bool randomPosition)
        {
            IInteractableItem interactableItem = _interactableItems[slotIndex];
            interactableItem?.DropServer(randomPosition); // Server RPC
            interactableItem?.Drop(randomPosition);

            _interactableItems[slotIndex] = null;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedSlotChanged(int selectedSlotIndex) => ToggleItemsState();

        private void OnItemDropped(int slotIndex, bool randomPosition) => DropItem(slotIndex, randomPosition);
    }
}