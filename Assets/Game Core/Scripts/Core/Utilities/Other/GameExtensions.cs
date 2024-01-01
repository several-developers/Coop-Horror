using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Utilities
{
    public static class GameExtensions
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static bool IsItem(this IInteractable interactable)
        {
            bool isItem = interactable is IInteractableItem;

            if (isItem)
                return true;
            
            string log = Log.HandleLog("Interactable <rb>is not item</rb>.");
            Debug.Log(log);

            return false;
        }
    }
}