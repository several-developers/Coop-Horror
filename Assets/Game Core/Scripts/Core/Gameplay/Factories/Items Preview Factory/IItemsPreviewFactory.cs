using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Items;
using GameCore.Utilities;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public interface IItemsPreviewFactory : IAddressablesFactory<int>
    {
        UniTaskVoid Create(ulong clientID, int itemID, bool isFirstPerson, Action<ItemPreviewObject> callbackEvent);
    }
}