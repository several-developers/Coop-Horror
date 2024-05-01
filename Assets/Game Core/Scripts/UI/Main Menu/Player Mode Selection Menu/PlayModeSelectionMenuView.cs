using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.PlayModeSelectionMenu
{
    public class PlayModeSelectionMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _onlineButton;
        
        [SerializeField, Required]
        private Button _offlineButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnOnlineClickedEvent;
        public event Action OnOfflineClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _onlineButton.onClick.AddListener(OnOnlineClicked);
            _offlineButton.onClick.AddListener(OnOfflineClicked);
            
            DestroyOnHide();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked()
        {
            Hide();
            OnOnlineClickedEvent?.Invoke();
        }

        private void OnOfflineClicked()
        {
            Hide();
            OnOfflineClickedEvent?.Invoke();
        }
    }
}