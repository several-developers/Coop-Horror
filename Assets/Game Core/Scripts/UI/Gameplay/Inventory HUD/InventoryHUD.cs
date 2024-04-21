using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.Observers.Gameplay.UI;
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
        private void Construct(IItemsMetaProvider itemsMetaProvider, IUIObserver uiObserver)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _uiObserver = uiObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemSlotView _itemSlotViewPrefab;

        [SerializeField, Required]
        private Transform _slotsContainer;

        [SerializeField, Required]
        private LayoutGroup _layoutGroup;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IItemsMetaProvider _itemsMetaProvider;
        private IUIObserver _uiObserver;

        private InventoryFactory _inventoryFactory;
        private LayoutFixHelper _layoutFixHelper;
        private PlayerEntity _playerEntity;
        private int _lastSlotIndex;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _inventoryFactory = new InventoryFactory(_itemSlotViewPrefab, _slotsContainer);
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _layoutGroup);

            _uiObserver.OnInitPlayerEvent += OnInitPlayer;
        }

        private void Start()
        {
            CreateItemsSlots();
            SelectSlot(slotIndex: 0);
            Show();
        }

        private void OnDestroy()
        {
            _uiObserver.OnInitPlayerEvent -= OnInitPlayer;

            if (!_isInitialized)
                return;
            
            PlayerInventory inventory = _playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent -= OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent -= OnItemEquipped;
            inventory.OnItemDroppedEvent -= OnItemDropped;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _isInitialized = true;

            PlayerInventory inventory = _playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent += OnItemEquipped;
            inventory.OnItemDroppedEvent += OnItemDropped;
        }
        
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

        private void SetIcon(int slotIndex, InventoryItemData inventoryItemData)
        {
            bool isItemSlotExists = GetItemSlot(slotIndex, out ItemSlotView itemSlotView);

            if (!isItemSlotExists)
                return;

            int itemID = inventoryItemData.ItemID;
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

        private void OnInitPlayer(PlayerEntity playerEntity) => Init(playerEntity);

        private void OnSelectedSlotChanged(int selectedSlotIndex) => SelectSlot(selectedSlotIndex);

        private void OnItemEquipped(int slotIndex, InventoryItemData inventoryItemData) => SetIcon(slotIndex, inventoryItemData);

        private void OnItemDropped(int slotIndex, bool randomPosition) => ClearSlot(slotIndex);
    }
}