using System;

namespace GameCore.Infrastructure.Services.Global.Decorators.CollectionsService
{
    public interface ICollectionsServiceDecorator
    {
        event Action<string, bool> OnAddItemEvent; 
        event Func<string> OnGetRandomItemIDEvent; 
        event Func<string, bool> OnIsItemUnlockedEvent;
        event Func<bool> OnIsAllItemsFoundedEvent;
        
        void AddItem(string itemID, bool autoSave = true);
        string GetRandomItemID();
        bool IsItemUnlocked(string itemID);
        bool IsAllItemsFounded();
    }
}