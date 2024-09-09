using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Items;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public interface IItemsPreviewFactory
    {
        UniTaskVoid Create(ulong clientID, int itemID, bool isFirstPerson, Action<ItemPreviewObject> callbackEvent);
    }
}