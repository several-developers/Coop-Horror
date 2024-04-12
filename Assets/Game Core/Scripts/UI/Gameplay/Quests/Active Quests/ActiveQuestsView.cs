using GameCore.Gameplay.Quests;
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
        private void Construct(IQuestsManagerDecorator questsManagerDecorator, IItemsMetaProvider itemsMetaProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;

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

        private ActiveQuestsFactory _activeQuestsFactory;
        private LayoutFixHelper _layoutFixHelper;
        private IQuestsManagerDecorator _questsManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _layoutFixHelper =
                new LayoutFixHelper(coroutineRunner: this, _activeQuestsLayoutGroup, _activeQuestsSizeFitter);

            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent += OnActiveQuestsDataReceived;
        }

        private void OnDestroy() =>
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent -= OnActiveQuestsDataReceived;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateActiveQuests()
        {
            _activeQuestsFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnActiveQuestsDataReceived()
        {
            CreateActiveQuests();
            Show();
        }
    }
}