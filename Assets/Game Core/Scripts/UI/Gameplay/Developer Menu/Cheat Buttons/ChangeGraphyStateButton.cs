using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.DeveloperMenu.CheatButtons
{
    public class ChangeGraphyStateButton : BaseButton
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => UpdateText();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic()
        {
          
            UpdateText();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateText()
        {
            bool isEnabled = false;
            string state = isEnabled ? "ON" : "OFF";
            _textTMP.text = $"Graphy <color=#FF2135>{state}</color>";
        }
    }
}