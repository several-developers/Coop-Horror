using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public interface IItemsPreviewFactory
    {
        bool Create(int itemID, Transform parent, out ItemPreviewObject itemPreviewObject);
    }
}