using GameCore.Gameplay.Observers;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.DeveloperMenu.CheatButtons
{
    public class ChangeGraphyStateButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGraphyStateObserver graphyStateObserver) =>
            _graphyStateObserver = graphyStateObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private IGraphyStateObserver _graphyStateObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => UpdateText();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic()
        {
            bool isEnabled = _graphyStateObserver.GetState();
            isEnabled = !isEnabled;

            _graphyStateObserver.SendChangeState(isEnabled);
            UpdateText();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateText()
        {
            bool isEnabled = _graphyStateObserver.GetState();
            string state = isEnabled ? "ON" : "OFF";
            _textTMP.text = $"Graphy <color=#FF2135>{state}</color>";
        }
    }
}