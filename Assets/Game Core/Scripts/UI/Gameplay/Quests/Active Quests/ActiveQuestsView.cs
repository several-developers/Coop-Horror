using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.Quests.ActiveQuests
{
    public class ActiveQuestsView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IQuestsManagerDecorator questsManagerDecorator,
            IGameManagerDecorator gameManagerDecorator, IItemsMetaProvider itemsMetaProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;

            _activeQuestsFactory = new ActiveQuestsFactory(questsManagerDecorator, itemsMetaProvider,
                _activeQuestViewPrefab, _activeQuestsContainer);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ActiveQuestView _activeQuestViewPrefab;

        [SerializeField, Required]
        private Transform _activeQuestsContainer;

        [SerializeField, Required]
        private LayoutGroup _activeQuestsLayoutGroup;

        [SerializeField, Required]
        private ContentSizeFitter _activeQuestsSizeFitter;

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private IGameManagerDecorator _gameManagerDecorator;
        private ActiveQuestsFactory _activeQuestsFactory;
        private LayoutFixHelper _layoutFixHelper;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _layoutFixHelper =
                new LayoutFixHelper(coroutineRunner: this, _activeQuestsLayoutGroup, _activeQuestsSizeFitter);

            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent += OnActiveQuestsDataReceived;
            _questsManagerDecorator.OnUpdateQuestsProgressEvent += OnUpdateQuestsProgress;

            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent -= OnActiveQuestsDataReceived;
            _questsManagerDecorator.OnUpdateQuestsProgressEvent -= OnUpdateQuestsProgress;
            
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateActiveQuests()
        {
            _activeQuestsFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.KillPlayersByMetroMonster:
                    Hide();
                    break;
                
                case GameState.RestartGame:
                    Show();
                    break;
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnActiveQuestsDataReceived()
        {
            CreateActiveQuests();
            Show();
        }

        private void OnUpdateQuestsProgress() =>
            _activeQuestsFactory.UpdateQuestsProgress();

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}