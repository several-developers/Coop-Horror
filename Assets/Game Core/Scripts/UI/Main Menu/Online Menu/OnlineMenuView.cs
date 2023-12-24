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
        private Button _hostButton;
        
        [SerializeField, Required]
        private Button _joinButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnBackButtonClickedEvent;
        public event Action OnHostClickedEvent;
        public event Action OnJoinClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
            
            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            Hide();
            OnBackButtonClickedEvent?.Invoke();
        }
        
        private void OnHostClicked()
        {
            Hide();
            OnHostClickedEvent?.Invoke();
        }

        private void OnJoinClicked()
        {
            Hide();
            OnJoinClickedEvent?.Invoke();
        }
    }
}