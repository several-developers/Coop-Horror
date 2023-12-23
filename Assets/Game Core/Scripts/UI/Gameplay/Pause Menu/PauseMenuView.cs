using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.PauseMenu
{
    public class PauseMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _continueButton;
        
        [SerializeField, Required]
        private Button _quitButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnContinueClickedEvent;
        public event Action OnQuitClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnContinueClicked() =>
            OnContinueClickedEvent?.Invoke();

        private void OnQuitClicked() =>
            OnQuitClickedEvent?.Invoke();
    }
}