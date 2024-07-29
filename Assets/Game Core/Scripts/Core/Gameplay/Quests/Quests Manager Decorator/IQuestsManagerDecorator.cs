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

        event Action OnCalculateRewardInnerEvent;
        event Action<int> OnSelectQuestInnerEvent;
        event Action<int> OnSubmitQuestItemInnerEvent;
        event Action OnCompleteQuestsInnerEvent;
        event Action OnDecreaseQuestsDaysInnerEvent;
        event Action OnResetQuestsInnerEvent;
        event Func<QuestsStorage> OnGetQuestsStorageInnerEvent;
        event Func<int> OnGetActiveQuestsAmountInnerEvent;
        event Func<int, bool> OnContainsItemInQuestsInnerEvent;
        event Func<bool> OnContainsCompletedQuestsInnerEvent;
        event Func<bool> OnContainsExpiredQuestsInnerEvent;
        event Func<bool> OnContainsExpiredAndUncompletedQuestsInnerEvent;
        void CalculateReward();
        void SelectQuest(int questID);
        void SubmitQuestItem(int itemID);
        void CompleteQuests();
        void DecreaseQuestsDays();
        void ResetQuests();
        QuestsStorage GetQuestsStorage();
        int GetActiveQuestsAmount();
        bool ContainsItemInQuests(int itemID);
        bool ContainsCompletedQuests();
        bool ContainsExpiredQuests();
        bool ContainsExpiredAndUncompletedQuests();
    }
}