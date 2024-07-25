using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.GameMap
{
    public class SectorButton : MapButtonBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private TextMeshProUGUI _titleTMP;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => UpdateTitle();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateTitle()
        {
            string title = LocationName.GetNiceName();
            _titleTMP.text = title;
        }
    }
}