using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.PauseMenu
{
    public class QuitConfirmMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _closeButton;
        
        [SerializeField, Required]
        private Button _confirmButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnConfirmClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => Hide();

        private void OnConfirmClicked() =>
            OnConfirmClickedEvent?.Invoke();
    }
}