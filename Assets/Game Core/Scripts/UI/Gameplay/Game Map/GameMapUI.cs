using GameCore.Enums.Gameplay;
using GameCore.Observers.Gameplay.UI;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.GameMap
{
    public class GameMapUI : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        [Inject]
        private void Construct(IUIManagerObserver uiManagerObserver, IUIObserver uiObserver)
        {
            _uiManagerObserver = uiManagerObserver;
            _uiObserver = uiObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _closeButton;
        
        // FIELDS: --------------------------------------------------------------------------------

        private IUIManagerObserver _uiManagerObserver;
        private IUIObserver _uiObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _uiObserver.OnTriggerUIEvent += OnTriggerUIEvent;
            
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _uiObserver.OnTriggerUIEvent -= OnTriggerUIEvent;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        protected override void OnShowMenu() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void OnTriggerUIEvent(UIEventType eventType)
        {
            switch (eventType)
            {
                case UIEventType.ShowGameMap:
                    Show();
                    break;
                
                case UIEventType.HideGameMap:
                    Hide();
                    break;
            }
        }
        
        private void OnCloseClicked() => Hide();
    }
}