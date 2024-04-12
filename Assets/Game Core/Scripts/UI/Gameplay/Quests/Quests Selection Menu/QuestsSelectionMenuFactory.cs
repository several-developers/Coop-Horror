using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Quests;
using UnityEngine;

namespace GameCore.UI.Gameplay.Quests
{
    public class QuestsSelectionMenuFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsSelectionMenuFactory(QuestsSelectionMenuView questsSelectionMenuView,
            IQuestsManagerDecorator questsManagerDecorator, Transform container,
            QuestItemButtonView questItemButtonPrefab)
        {
            _questsSelectionMenuView = questsSelectionMenuView;
            _questsManagerDecorator = questsManagerDecorator;
            _container = container;
            _questItemButtonPrefab = questItemButtonPrefab;
            _questItemsButtons = new List<QuestItemButtonView>(capacity: 5);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly QuestsSelectionMenuView _questsSelectionMenuView;
        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly Transform _container;
        private readonly QuestItemButtonView _questItemButtonPrefab;
        private readonly List<QuestItemButtonView> _questItemsButtons;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Create()
        {
            ClearQuestsItemsButtons();
            CreationLogic();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreationLogic()
        {
            QuestsStorage questsStorage = _questsManagerDecorator.GetQuestsStorage();
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetAwaitingQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                QuestItemButtonView questItemButton = Object.Instantiate(_questItemButtonPrefab, _container);
                int questID = questRuntimeData.QuestID;
                int itemQuantity = questRuntimeData.GetItemsTotalAmount();
                QuestDifficulty questDifficulty = questRuntimeData.Difficulty;

                questItemButton.Setup(questID, itemQuantity, questDifficulty);
                questItemButton.OnQuestItemClickedEvent += OnQuestItemClicked;

                _questItemsButtons.Add(questItemButton);
            }
        }

        private void ClearQuestsItemsButtons()
        {
            foreach (QuestItemButtonView questItemButtonView in _questItemsButtons)
                Object.Destroy(questItemButtonView.gameObject);

            _questItemsButtons.Clear();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnQuestItemClicked(int questID)
        {
            _questsManagerDecorator.SelectQuest(questID);
            _questsSelectionMenuView.Hide();
        }
    }
}