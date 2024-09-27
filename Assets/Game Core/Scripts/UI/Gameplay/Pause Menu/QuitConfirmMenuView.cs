using System;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.PauseMenu
{
    public class QuitConfirmMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IUIManagerObserver uiManagerObserver) =>
            _uiManagerObserver = uiManagerObserver;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _closeButton;
        
        [SerializeField, Required]
        private Button _confirmButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnConfirmClickedEvent;

        private IUIManagerObserver _uiManagerObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _closeButton.onClick.AddListener(OnCloseClicked);
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnShowMenu() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void OnCloseClicked() => Hide();

        private void OnConfirmClicked() =>
            OnConfirmClickedEvent?.Invoke();
    }
}