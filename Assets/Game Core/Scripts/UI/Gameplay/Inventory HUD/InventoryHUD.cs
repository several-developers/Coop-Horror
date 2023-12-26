using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
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

        private static InventoryHUD _instance; // TEMP
        
        private InventoryFactory _inventoryFactory;
        private LayoutFixHelper _layoutFixHelper;
        private PlayerEntity _playerEntity;
        private int _lastSlotIndex;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            _inventoryFactory = new InventoryFactory(_itemSlotViewPrefab, _slotsContainer);
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _layoutGroup);
        }

        private void Start()
        {
            CreateItemsSlots();
            SelectSlot(slotIndex: 0);
            Show();
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
                return;
            
            Inventory<ItemData> inventory = _playerEntity.GetInventory();
            inventory.OnSelectedSlotChangedEvent -= OnSelectedSlotChanged;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _isInitialized = true;

            Inventory<ItemData> inventory = _playerEntity.GetInventory();
            inventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
        }
        
        public static InventoryHUD Get() => _instance;
        
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedSlotChanged(int selectedSlotIndex) => SelectSlot(selectedSlotIndex);
    }
}