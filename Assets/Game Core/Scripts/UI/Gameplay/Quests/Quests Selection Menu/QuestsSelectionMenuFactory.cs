using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Quests;
using UnityEngine;

namespace GameCore.UI.Gameplay.Quests
{
    public class QuestsSelectionMenuFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsSelectionMenuFactory(IQuestsManagerDecorator questsManagerDecorator,
            Transform questsItemsContainer, QuestItemButtonView questItemButtonPrefab)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _questsItemsContainer = questsItemsContainer;
            _questItemButtonPrefab = questItemButtonPrefab;
            _questItemsButtons = new List<QuestItemButtonView>(capacity: 5);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly Transform _questsItemsContainer;
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
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                QuestItemButtonView questItemButton = Object.Instantiate(_questItemButtonPrefab, _questsItemsContainer);
                int questID = questRuntimeData.QuestID;
                int reward = questRuntimeData.Reward;
                int itemQuantity = questRuntimeData.GetItemsTotalAmount();
                QuestDifficulty questDifficulty = questRuntimeData.Difficulty;
                
                questItemButton.Setup(questID, itemQuantity, questDifficulty);
                
                _questItemsButtons.Add(questItemButton);
            }
        }

        private void ClearQuestsItemsButtons()
        {
            foreach (QuestItemButtonView questItemButtonView in _questItemsButtons)
                Object.Destroy(questItemButtonView.gameObject);
            
            _questItemsButtons.Clear();
        }
    }
}