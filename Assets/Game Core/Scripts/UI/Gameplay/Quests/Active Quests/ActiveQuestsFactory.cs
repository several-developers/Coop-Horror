using System.Collections.Generic;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using UnityEngine;

namespace GameCore.UI.Gameplay.Quests.ActiveQuests
{
    public class ActiveQuestsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ActiveQuestsFactory(IQuestsManagerDecorator questsManagerDecorator, IItemsMetaProvider itemsMetaProvider,
            ActiveQuestView activeQuestViewPrefab, Transform container)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _itemsMetaProvider = itemsMetaProvider;
            _activeQuestViewPrefab = activeQuestViewPrefab;
            _container = container;
            _activeQuestsList = new List<ActiveQuestView>(capacity: 6);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly ActiveQuestView _activeQuestViewPrefab;
        private readonly Transform _container;
        private readonly List<ActiveQuestView> _activeQuestsList;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Create()
        {
            Clear();
            CreationLogic();
        }

        public void UpdateQuestsProgress()
        {
            foreach (ActiveQuestView activeQuestView in _activeQuestsList)
            {
                activeQuestView.UpdateQuestProgress();
                activeQuestView.UpdateQuestItemsInfo();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreationLogic()
        {
            QuestsStorage questsStorage = _questsManagerDecorator.GetQuestsStorage();
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                ActiveQuestView activeQuestView = Object.Instantiate(_activeQuestViewPrefab, _container);
                activeQuestView.Setup(_itemsMetaProvider, questRuntimeData);

                _activeQuestsList.Add(activeQuestView);
            }
        }

        private void Clear()
        {
            foreach (ActiveQuestView activeQuestView in _activeQuestsList)
                Object.Destroy(activeQuestView.gameObject);

            _activeQuestsList.Clear();
        }
    }
}