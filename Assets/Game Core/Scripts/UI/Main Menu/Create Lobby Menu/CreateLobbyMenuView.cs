using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.CreateLobbyMenu
{
    public class CreateLobbyMenuView : MenuView
    {
        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _startGameButton;

        public event Action OnStartGameClickedEvent;

        private void Awake()
        {
            _startGameButton.onClick.AddListener(OnStartGameClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        private void OnStartGameClicked()
        {
            OnStartGameClickedEvent?.Invoke();
            Hide();
        }
    }
}