using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.CreateLobbyMenu
{
    public class CreateLobbyMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _startGameButton;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnStartGameClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            _startGameButton.onClick.AddListener(OnStartGameClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnStartGameClicked()
        {
            OnStartGameClickedEvent?.Invoke();
            Hide();
        }
    }
}