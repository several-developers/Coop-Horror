using System;
using GameCore.Enums.Gameplay;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.LocationsSelectionMenu
{
    public class LocationItemButtonView : BaseButton
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _titleTMP;

        [SerializeField, Required]
        private Image _defaultBackground;
        
        [SerializeField, Required]
        private Image _selectedBackground;

        // PROPERTIES: ----------------------------------------------------------------------------

        public LocationName LocationName { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<LocationName> OnLocationItemClickedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(LocationName locationName, string locationNameText)
        {
            LocationName = locationName;
            _titleTMP.text = $"Location: {locationNameText}";
        }

        public void ToggleSelected(bool isSelected)
        {
            _defaultBackground.enabled = !isSelected;
            _selectedBackground.enabled = isSelected;
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic() =>
            OnLocationItemClickedEvent?.Invoke(LocationName);
    }
}