using GameCore.Enums;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public abstract class ItemObjectBase : MonoBehaviour, IInteractableItem
    {
        // FIELDS: --------------------------------------------------------------------------------

        private int _itemID;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int itemID) =>
            _itemID = itemID;

        public virtual void Interact()
        {
            Debug.Log("Interacting with: " + name);
        }

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;

        public int GetItemID() => _itemID;
    }
}