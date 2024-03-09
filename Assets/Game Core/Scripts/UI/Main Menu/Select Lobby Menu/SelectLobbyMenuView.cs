using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.SelectLobbyMenu
{
    public class SelectLobbyMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _startWithLobbyButton;
        
        [SerializeField, Required]
        private Button _startWithDirectIPButton;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public event Action OnStartWithLobbyClickedEvent; 
        public event Action OnStartWithDirectIPClickedEvent; 

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _startWithLobbyButton.onClick.AddListener(OnStartWithLobbyClicked);
            _startWithDirectIPButton.onClick.AddListener(OnStartWithDirectIPClicked);
            
            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartWithLobbyClicked()
        {
            OnStartWithLobbyClickedEvent?.Invoke();
            Hide();
        }

        private void OnStartWithDirectIPClicked()
        {
            OnStartWithDirectIPClickedEvent?.Invoke();
            Hide();
        }
    }
}