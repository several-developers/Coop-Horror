using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.GameOverWarningMenu
{
    public class GameOverWarningMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _confirmButton;
        
        [SerializeField, Required]
        private Button _cancelButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnConfirmClickedEvent = delegate { };
        public event Action OnCancelClickedEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnConfirmClicked()
        {
            Hide();
            OnConfirmClickedEvent.Invoke();
        }

        private void OnCancelClicked()
        {
            Hide();
            OnCancelClickedEvent.Invoke();
        }
    }
}