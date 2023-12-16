
using System;

namespace GameCore.Infrastructure.Services.Global.Decorators.CollectionsService
{
    public class CollectionsServiceDecorator : ICollectionsServiceDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<string, bool> OnAddItemEvent;
        public event Func<string> OnGetRandomItemIDEvent;
        public event Func<string, bool> OnIsItemUnlockedEvent;
        public event Func<bool> OnIsAllItemsFoundedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AddItem(string itemID, bool autoSave = true) =>
            OnAddItemEvent?.Invoke(itemID, autoSave);

        public string GetRandomItemID() =>
            OnGetRandomItemIDEvent?.Invoke();

        public bool IsItemUnlocked(string itemID)
        {
            if (OnIsItemUnlockedEvent == null)
                return false;
            
            return OnIsItemUnlockedEvent(itemID);
        }

        public bool IsAllItemsFounded()
        {
            if (OnIsAllItemsFoundedEvent == null)
                return true;

            bool isAllItemsFound = OnIsAllItemsFoundedEvent();
            return isAllItemsFound;
        }
    }
}