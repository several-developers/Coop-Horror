using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.OfflineMenu
{
    public class OfflineMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _startGameButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnStartButtonClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartGameButtonClicked()
        {
            OnStartButtonClickedEvent?.Invoke();
            Hide();
        }
    }
}