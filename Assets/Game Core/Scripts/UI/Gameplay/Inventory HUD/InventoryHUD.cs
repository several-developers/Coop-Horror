using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameManagement;
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
        private void Construct(IGameManagerDecorator gameManagerDecorator, IItemsMetaProvider itemsMetaProvider)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _itemsMetaProvider = itemsMetaProvider;
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

        private IGameManagerDecorator _gameManagerDecorator;
        private IItemsMetaProvider _itemsMetaProvider;

        private InventoryFactory _inventoryFactory;
        private LayoutFixHelper _layoutFixHelper;
        private int _lastSlotIndex;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _inventoryFactory = new InventoryFactory(_itemSlotViewPrefab, _slotsContainer);
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _layoutGroup);

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
            
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        private void Start()
        {
            CreateItemsSlots();
            SelectSlot(slotIndex: 0);
            Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
            
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.KillPlayersByMetroMonster:
                    Hide();
                    break;
                
                case GameState.RestartGame:
                    Show();
                    break;
            }
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

        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            PlayerInventory inventory = playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent += OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent += OnItemEquipped;
            inventory.OnItemDroppedEvent += OnItemDropped;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;
            
            PlayerInventory inventory = playerEntity.GetInventory();

            inventory.OnSelectedSlotChangedEvent -= OnSelectedSlotChanged;
            inventory.OnItemEquippedEvent -= OnItemEquipped;
            inventory.OnItemDroppedEvent -= OnItemDropped;
        }
        
        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
        
        private void OnSelectedSlotChanged(ChangedSlotStaticData data) => SelectSlot(data.SlotIndex);

        private void OnItemEquipped(EquippedItemStaticData data) => SetIcon(data.SlotIndex, data.InventoryItemData);

        private void OnItemDropped(DroppedItemStaticData data) => ClearSlot(data.SlotIndex);
    }
}