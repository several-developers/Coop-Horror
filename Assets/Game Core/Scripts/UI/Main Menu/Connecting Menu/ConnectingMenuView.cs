using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.ConnectingMenu
{
    public class ConnectingMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _quitButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnQuitClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _quitButton.onClick.AddListener(OnQuitClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnQuitClicked()
        {
            OnQuitClickedEvent?.Invoke();
            Hide();
        }
    }
}