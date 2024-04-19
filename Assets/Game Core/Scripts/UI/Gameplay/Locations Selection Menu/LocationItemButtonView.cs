using System;
using GameCore.Enums.Global;
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

        public SceneName SceneName { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<SceneName> OnLocationItemClickedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(SceneName sceneName, string locationName)
        {
            SceneName = sceneName;
            _titleTMP.text = $"Location: {locationName}";
        }

        public void ToggleSelected(bool isSelected)
        {
            _defaultBackground.enabled = !isSelected;
            _selectedBackground.enabled = isSelected;
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic() =>
            OnLocationItemClickedEvent?.Invoke(SceneName);
    }
}