using System;

namespace GameCore.Gameplay.Quests
{
    public interface IQuestsManagerDecorator
    {
        event Action OnAwaitingQuestsDataReceivedEvent; 
        event Action OnActiveQuestsDataReceivedEvent; 
        event Action OnUpdateQuestsProgressEvent;
        void AwaitingQuestsDataReceived();
        void ActiveQuestsDataReceived();
        void UpdateQuestsProgress();
        
        event Action<int> OnSelectQuestInnerEvent;
        event Action<int> OnSubmitQuestItemInnerEvent;
        event Action OnCompleteQuestsInnerEvent;
        event Func<QuestsStorage> OnGetQuestsStorageInnerEvent;
        event Func<int> OnGetActiveQuestsAmountInnerEvent;
        event Func<bool> OnContainsCompletedQuestsInnerEvent;
        event Func<bool> OnContainsExpiredQuestsInnerEvent;
        event Func<bool> OnContainsExpiredAndUncompletedQuestsInnerEvent;
        void SelectQuest(int questID);
        void SubmitQuestItem(int itemID);
        void CompleteQuests();
        QuestsStorage GetQuestsStorage();
        int GetActiveQuestsAmount();
        bool ContainsCompletedQuests();
        bool ContainsExpiredQuests();
        bool ContainsExpiredAndUncompletedQuests();
    }
}