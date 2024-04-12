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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(Sprite itemIcon, string itemName, int itemQuantity)
        {
            _itemIcon.sprite = itemIcon;
            _titleTMP.text = $"{itemName} - x{itemQuantity}";
        }
    }
}