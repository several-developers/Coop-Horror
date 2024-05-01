using System;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.PauseMenu
{
    public class PauseMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IUIManagerObserver uiManagerObserver) =>
            _uiManagerObserver = uiManagerObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _continueButton;
        
        [SerializeField, Required]
        private Button _quitButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnContinueClickedEvent;
        public event Action OnQuitClickedEvent;

        private IUIManagerObserver _uiManagerObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _continueButton.onClick.AddListener(OnContinueClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnShowMenu() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void OnContinueClicked() =>
            OnContinueClickedEvent?.Invoke();

        private void OnQuitClicked() =>
            OnQuitClickedEvent?.Invoke();
    }
}