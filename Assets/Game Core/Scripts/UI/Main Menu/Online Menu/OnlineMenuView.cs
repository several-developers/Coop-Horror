using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.OnlineMenu
{
    public class OnlineMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _backButton;
        
        [SerializeField, Required]
        private Button _openRelayLobbyMenuButton;
        
        [SerializeField, Required]
        private Button _openIPLobbyMenuButton;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnBackButtonClickedEvent;
        public event Action OnOpenRelayLobbyMenuClickedEvent;
        public event Action OnOpenIPLobbyMenuClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _openRelayLobbyMenuButton.onClick.AddListener(OnOpenRelayLobbyMenuClicked);
            _openIPLobbyMenuButton.onClick.AddListener(OnOpenIPLobbyMenuClicked);
            
            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            Hide();
            OnBackButtonClickedEvent?.Invoke();
        }
        
        private void OnOpenRelayLobbyMenuClicked()
        {
            Hide();
            OnOpenRelayLobbyMenuClickedEvent?.Invoke();
        }

        private void OnOpenIPLobbyMenuClicked()
        {
            Hide();
            OnOpenIPLobbyMenuClickedEvent?.Invoke();
        }
    }
}