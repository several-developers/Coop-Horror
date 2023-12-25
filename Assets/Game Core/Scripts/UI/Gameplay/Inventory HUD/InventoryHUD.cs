using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.Inventory
{
    public class InventoryHUD : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemSlotView _itemSlotViewPrefab;

        [SerializeField, Required]
        private Transform _slotsContainer;

        [SerializeField, Required]
        private LayoutGroup _layoutGroup;

        // FIELDS: --------------------------------------------------------------------------------

        private InventoryFactory _inventoryFactory;
        private LayoutFixHelper _layoutFixHelper;
        private int _lastSlotIndex;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _inventoryFactory = new InventoryFactory(_itemSlotViewPrefab, _slotsContainer);
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _layoutGroup);
        }

        private void Start()
        {
            CreateItemsSlots();
            SelectSlot(slotIndex: 0);
            Show();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateItemsSlots()
        {
            _inventoryFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        private void SelectSlot(int slotIndex)
        {
            bool isSlotExists = slotIndex < Constants.PlayerInventorySize;

            if (!isSlotExists)
                slotIndex = 0;
            
            bool isItemSlotFound = GetItemSlot(slotIndex, out ItemSlotView itemSlotView);

            if (!isItemSlotFound)
                return;

            DeselectLastSlot();
            itemSlotView.Select();
            
            _lastSlotIndex = slotIndex;
        }

        private void DeselectLastSlot()
        {
            bool isItemSlotFound = GetItemSlot(_lastSlotIndex, out ItemSlotView itemSlotView);

            if (!isItemSlotFound)
                return;
            
            itemSlotView.Deselect();
        }

        private bool GetItemSlot(int slotIndex, out ItemSlotView itemSlotView) =>
            _inventoryFactory.GetItemSlot(slotIndex, out itemSlotView);
    }
}