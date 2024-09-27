using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.Inventory
{
    [Serializable]
    public class ItemSlotVisualizer
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Image _selectedBackgroundImage;
        
        [SerializeField, Required]
        private Image _selectedFrameImage;
        
        [SerializeField, Required]
        private Image _iconImage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Select() => SetSelectedState(isSelected: true);

        public void Deselect() => SetSelectedState(isSelected: false);

        public void SetIcon(Sprite sprite)
        {
            _iconImage.sprite = sprite;
            _iconImage.enabled = true;
        }

        public void RemoveIcon() =>
            _iconImage.enabled = false;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetSelectedState(bool isSelected)
        {
            _selectedBackgroundImage.enabled = isSelected;
            _selectedFrameImage.enabled = isSelected;
        }
    }
}