using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public interface IItemsPreviewFactory
    {
        bool Create(ulong clientID, int itemID, bool isFirstPerson, out ItemPreviewObject itemPreviewObject);
    }
}