using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Gameplay.DeveloperMenu
{
    [Serializable]
    public class CheatButtonsReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private TabType _tabType;

        [SerializeField, Required, Space(5)]
        private GameObject[] _cheatButtons;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TabType TabType => _tabType;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetButtonsState(bool isEnabled)
        {
            foreach (GameObject cheatButton in _cheatButtons)
                cheatButton.SetActive(isEnabled);
        }
    }
}