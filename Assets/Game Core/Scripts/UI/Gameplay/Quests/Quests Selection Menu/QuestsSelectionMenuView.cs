using GameCore.Gameplay.Quests;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.Quests
{
    public class QuestsSelectionMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IQuestsManagerDecorator questsManagerDecorator, IUIManagerObserver uiManagerObserver)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _uiManagerObserver = uiManagerObserver;

            _questsSelectionMenuFactory = new QuestsSelectionMenuFactory(questsManagerDecorator, _questsItemsContainer,
                _questItemButtonPrefab);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _questsItemsContainer;

        [SerializeField, Required]
        private QuestItemButtonView _questItemButtonPrefab;

        [SerializeField, Required]
        private LayoutGroup _questsItemsLayoutGroup;

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private IUIManagerObserver _uiManagerObserver;
        private QuestsSelectionMenuFactory _questsSelectionMenuFactory;
        private LayoutFixHelper _layoutFixHelper;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _questsItemsLayoutGroup);

            _questsManagerDecorator.OnAwaitingQuestsDataReceivedEvent += AwaitingQuestsDataReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _questsManagerDecorator.OnAwaitingQuestsDataReceivedEvent -= AwaitingQuestsDataReceived;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            CreateQuestsItemsButtons();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateQuestsItemsButtons()
        {
            _questsSelectionMenuFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnShowMenu() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void AwaitingQuestsDataReceived() => CreateQuestsItemsButtons();
    }
}