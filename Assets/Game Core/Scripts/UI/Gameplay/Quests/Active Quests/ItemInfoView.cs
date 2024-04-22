using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.Quests.ActiveQuests
{
    public class ItemInfoView : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Image _itemIcon;

        [SerializeField, Required]
        private TextMeshProUGUI _titleTMP;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public int ItemID { get; private set; }

        private string _itemName;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(Sprite itemIcon, string itemName, int itemQuantity, int itemID)
        {
            _itemName = itemName;
            ItemID = itemID;
            _itemIcon.sprite = itemIcon;
            
            UpdateTitle(itemQuantity);
        }

        public void UpdateTitle(int itemQuantity) =>
            _titleTMP.text = $"{_itemName} - x{itemQuantity}";
    }
}