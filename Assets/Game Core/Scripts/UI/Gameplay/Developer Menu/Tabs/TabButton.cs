using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.DeveloperMenu
{
    public class TabButton : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private TabType _tabType;

        [SerializeField]
        private Color _defaultColor;

        [SerializeField]
        private Color _defaultShadowColor;

        [SerializeField]
        private Color _selectedColor;

        [SerializeField]
        private Color _selectedShadowColor;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _button;

        [SerializeField, Required]
        private Image _backgroundImage;

        [SerializeField, Required]
        private Shadow _backgroundShadow;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TabType TabType => _tabType;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TabType> OnTabClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _button.onClick.AddListener(OnTabClicked);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetSelectedState(bool isSelected)
        {
            Color backgroundColor = isSelected ? _selectedColor : _defaultColor;
            Color backgroundShadowColor = isSelected ? _selectedShadowColor : _defaultShadowColor;

            _backgroundImage.color = backgroundColor;
            _backgroundShadow.effectColor = backgroundShadowColor;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTabClicked() =>
            OnTabClickedEvent?.Invoke(_tabType);
    }
}