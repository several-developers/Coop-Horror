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

        public void Setup(Sprite itemIcon, string itemName, int targetItemQuantity, int currentItemQuantity, int itemID)
        {
            _itemName = itemName;
            ItemID = itemID;
            _itemIcon.sprite = itemIcon;

            UpdateTitle(targetItemQuantity, currentItemQuantity);
        }

        public void UpdateTitle(int targetItemQuantity, int currentItemQuantity) =>
            _titleTMP.text = $"{_itemName} - {currentItemQuantity}/{targetItemQuantity}";
    }
}