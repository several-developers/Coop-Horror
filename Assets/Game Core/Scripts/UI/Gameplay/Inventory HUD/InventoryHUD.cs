using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.Inventory
{
    public class InventoryHUD : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsMetaProvider itemsMetaProvider) =>
            _itemsMetaProvider = itemsMetaProvider;

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

        private IItemsMetaProvider _itemsMetaProvider;

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

            PlayerInventory inventory = _playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent -= OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent -= OnItemEquipped;
            inventory.OnItemDroppedEvent -= OnItemDropped;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _isInitialized = true;

            PlayerInventory inventory = _playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent += OnItemEquipped;
            inventory.OnItemDroppedEvent += OnItemDropped;
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

        private void SetIcon(int slotIndex, ItemData itemData)
        {
            bool isItemSlotExists = GetItemSlot(slotIndex, out ItemSlotView itemSlotView);

            if (!isItemSlotExists)
                return;

            int itemID = itemData.ItemID;
            bool isItemMetaFound = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaFound)
                return;
            
            itemSlotView.SetIcon(itemMeta.Icon);
        }

        private void ClearSlot(int slotIndex)
        {
            bool isItemSlotExists = GetItemSlot(slotIndex, out ItemSlotView itemSlotView);

            if (!isItemSlotExists)
                return;

            itemSlotView.RemoveIcon();
        }

        private bool GetItemSlot(int slotIndex, out ItemSlotView itemSlotView) =>
            _inventoryFactory.GetItemSlot(slotIndex, out itemSlotView);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedSlotChanged(int selectedSlotIndex) => SelectSlot(selectedSlotIndex);

        private void OnItemEquipped(int slotIndex, ItemData itemData) => SetIcon(slotIndex, itemData);

        private void OnItemDropped(int slotIndex, bool randomPosition) => ClearSlot(slotIndex);
    }
}