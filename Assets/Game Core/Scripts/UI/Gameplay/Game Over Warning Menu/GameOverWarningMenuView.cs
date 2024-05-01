using System;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.GameOverWarningMenu
{
    public class GameOverWarningMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IUIManagerObserver uiManagerObserver) =>
            _uiManagerObserver = uiManagerObserver;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _confirmButton;
        
        [SerializeField, Required]
        private Button _cancelButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnConfirmClickedEvent = delegate { };
        public event Action OnCancelClickedEvent = delegate { };

        private IUIManagerObserver _uiManagerObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnMenuShown() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void OnConfirmClicked()
        {
            Hide();
            OnConfirmClickedEvent.Invoke();
        }

        private void OnCancelClicked()
        {
            Hide();
            OnCancelClickedEvent.Invoke();
        }
    }
}