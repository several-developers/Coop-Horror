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
        private Button _backGameButton;
        
        [SerializeField, Required]
        private Button _startGameButton;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnBackButtonClickedEvent;
        public event Action OnStartGameClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _backGameButton.onClick.AddListener(OnBackButtonClicked);
            _startGameButton.onClick.AddListener(OnStartGameClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            Hide();
            OnBackButtonClickedEvent?.Invoke();
        }
        
        private void OnStartGameClicked()
        {
            Hide();
            OnStartGameClickedEvent?.Invoke();
        }
    }
}