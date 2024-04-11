using System;
using GameCore.Enums.Gameplay;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.Quests
{
    public class QuestItemButtonView : BaseButton
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _titleTMP;
        
        [SerializeField, Required]
        private TextMeshProUGUI _infoTMP;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<int> OnQuestItemClickedEvent; 

        private int _questID;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int questID, int itemQuantity, QuestDifficulty questDifficulty)
        {
            _questID = questID;
            _titleTMP.text = $"Quest #{questID}";
            _infoTMP.text = $"Items: {itemQuantity},  Difficulty: {questDifficulty}";
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic() =>
            OnQuestItemClickedEvent?.Invoke(_questID);
    }
}