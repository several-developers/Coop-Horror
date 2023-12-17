using System;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.SaveSelectionMenu
{
    public class SaveCellView : BaseButton
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Color _defaultColor;
        
        [SerializeField]
        private Color _selectedColor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Image _backgroundImage;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<int> OnSaveCellClickedEvent;

        private int _cellIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetCellIndex(int index) =>
            _cellIndex = index;

        public void Select() => ChangeBackgroundColor(_selectedColor);

        public void Deselect() => ChangeBackgroundColor(_defaultColor);

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic() =>
            OnSaveCellClickedEvent?.Invoke(_cellIndex);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeBackgroundColor(Color color) =>
            _backgroundImage.color = color;
    }
}