using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Inventory;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using Zenject;

namespace GameCore.Gameplay.Interactable.MarketShop
{
    public class MarketShopTrigger : IInteractable
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

        public void InteractionStarted()
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            playerInventory.OnSelectedSlotChangedEvent += OnPlayerSelectedSlotChanged;
        }

        public void InteractionEnded()
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            playerInventory.OnSelectedSlotChangedEvent -= OnPlayerSelectedSlotChanged;
        }

        public void Interact(IEntity entity = null)
        {
            throw new NotImplementedException();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            
        }

        public InteractionType GetInteractionType()
        {
            throw new NotImplementedException();
        }

        public bool CanInteract()
        {
            PlayerInventory playerInventory = GetPlayerInventory();
            
            bool isItemInSelectedSlotExists =
                playerInventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

            if (!isItemInSelectedSlotExists)
                return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static PlayerInventory GetPlayerInventory()
        {
            PlayerEntity localPlayer = GetLocalPlayer();
            return localPlayer.GetInventory();
        }

        private static PlayerEntity GetLocalPlayer() =>
            PlayerEntity.GetLocalPlayer();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerSelectedSlotChanged()
        {
        }
    }
}