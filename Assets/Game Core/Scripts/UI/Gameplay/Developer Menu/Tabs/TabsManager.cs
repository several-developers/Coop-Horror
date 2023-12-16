using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.DeveloperMenu
{
    public class TabsManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private TabType _startTabType = TabType.None;
        
        [SerializeField, Space(5)]
        private TabButton[] _tabButtons;

        [SerializeField]
        private CheatButtonsReferences[] _cheatButtonsReferences;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Image _background;

        // FIELDS: --------------------------------------------------------------------------------

        private TabType _previousTabType;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            _previousTabType = _startTabType;
            
            foreach (TabButton tabButton in _tabButtons)
                tabButton.OnTabClickedEvent += OnTabClicked;

            ChangeTab(_startTabType);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeTab(TabType tabType)
        {
            _background.enabled = tabType != TabType.None;
            _previousTabType = tabType;
            
            foreach (TabButton tabButton in _tabButtons)
            {
                bool isSelected = tabButton.TabType == tabType;
                tabButton.SetSelectedState(isSelected);
            }

            foreach (CheatButtonsReferences cheatButtonsReference in _cheatButtonsReferences)
            {
                bool isEnabled = cheatButtonsReference.TabType == tabType;
                cheatButtonsReference.SetButtonsState(isEnabled);
            }
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTabClicked(TabType tabType)
        {
            if (tabType == _previousTabType)
                tabType = TabType.None;
            
            ChangeTab(tabType);
        }
    }
}